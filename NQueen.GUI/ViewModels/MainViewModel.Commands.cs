namespace NQueen.GUI.ViewModels;

public sealed partial class MainViewModel
{
    // ----------------- PUBLIC / COMMAND TARGET METHODS -----------------

    private async Task SimulateAsync()
    {
        if (IsSimulating)
            return;

        if (ParsingUtils.TryParseInt(BoardSizeText, out var boardSize) == false)
            return;

        if (DisplayMode == DisplayMode.Visualize &&
            boardSize > SimulationSettings.MaxVisualizeBoardSize)
        {
            if (SuppressUserDialogs == false)
            {
                MessageBox.Show(ErrorMessages.VisualizeSizeTooLarge,
                    "Visualization Limit", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            DisplayMode = DisplayMode.Hide;
        }

        if (_solver is Kernel.Solvers.BitmaskSolver bitmask)
        {
            Kernel.Solvers.BitmaskSolverRunConfigurator.Configure(
                bitmask,
                boardSize,
                SolutionMode,
                DisplayMode,
                SolutionMode is SolutionMode.All or SolutionMode.Single ? SelectedStorageMode : bitmask.AllStorageMode,
                SolutionMode == SolutionMode.Unique ? SelectedStorageMode : bitmask.UniqueStorageMode);

            UseParallel = bitmask.UseParallel;
            ParallelRootSplitDepth = bitmask.ParallelRootSplitDepth;
        }

        // Capture the token up front: Cancel() disposes and replaces the CTS, so reading
        // CancellationTokenSource.Token after the await could observe a fresh, uncancelled source.
        // The captured token is the single cancellation source of truth for the run — the solver
        // reads it via SimulationContext.Cancellation, and every post-await guard below reads the
        // same local so a mid-await Cancel() (which replaces the CTS) is still observed.
        var cancellationToken = CancellationTokenSource.Token;

        // High-frequency QueenPlaced stream (Stage 4): a conflating, keep-latest channel drained by
        // the visualization DispatcherTimer. Only wired in Visualize mode; Hide/CountOnly runs leave
        // it null so the solver pays no copy cost. Declared here so it can be completed in finally.
        Channel<QueenPlacedInfo>? queenChannel = null;

        try
        {
            // Pause gate is only meaningful for the animated Visualized Single (N <= 8) path.
            // Signaled = running; the Stop/Resume button flips it. Null otherwise so nothing blocks.
            // Created BEFORE the Started status transition so CanTogglePause sees a non-null gate
            // when RefreshCommandStates first evaluates the toggle button's enabled state.
            IsPaused = false;
            _pauseGate?.Dispose();
            _pauseGate = (DisplayMode == DisplayMode.Visualize &&
                          SolutionMode == SolutionMode.Single &&
                          boardSize <= SimulationSettings.MaxVisualizeSingleBoardSize)
                ? new ManualResetEventSlim(initialState: true)
                : null;

            ResetSimulationState();
            ManageSimulationStatus(SimulationStatus.Started);
            UpdateUiState();

            var progress = new Progress<ProgressInfo>(OnProgressReported);
            var solutionSink = new SynchronousProgress<SolutionFoundInfo>(OnSolutionFoundReported);

            if (DisplayMode == DisplayMode.Visualize)
            {
                // When a delay is set we want a faithful, in-order animation: every placement and
                // backtrack the engine emits must be rendered, one step per timer tick. A conflating
                // (drop-oldest) channel would discard intermediate frames and make the board "jump
                // over stages" because the engine runs ahead of the UI. Use an unbounded FIFO so no
                // frame is lost; the engine's own Thread.Sleep keeps the queue small.
                // With no delay we only care about the latest prefix, so keep the conflating fast path.
                queenChannel = DelayInMilliseconds > 0
                    ? Channel.CreateUnbounded<QueenPlacedInfo>(
                        new UnboundedChannelOptions { SingleReader = true })
                    : Channel.CreateBounded<QueenPlacedInfo>(
                        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
                StartQueenPlacedDrain(queenChannel.Reader);
            }

            var simContext = new SimulationContext(
                boardSize, SolutionMode, DisplayMode, progress, cancellationToken, solutionSink,
                queenChannel?.Writer, _pauseGate);
            _solver.DelayInMillisec = DelayInMilliseconds;

            SimulationResults = await _solver.GetSimResultsAsync(simContext);

            if (cancellationToken.IsCancellationRequested)
                return;

            if (SimulationResults == null)
                throw new InvalidOperationException("No solutions were generated by the solver.");

            ExtractCorrectNoOfSols();
            NoOfSolutions = NumericUtils.FormatWithSpaceSeparator(SimulationResults.SolutionsCount);
            ElapsedTimeInSec = $"{SimulationResults.ElapsedTimeInSec,0:N1}";
            MemoryConsumption = NumericUtils.UpdateMemoryUsage();

            if (ObservableSolutions.Count > 0)
                SelectedSolution = ObservableSolutions[0];

            IsOutputReady = ObservableSolutions.Count > 0;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.WriteLine($"[SimulateAsync] Suppressed exception after cancel: {ex.Message}");
            }
            else
            {
                Debug.WriteLine($"[SimulateAsync] Exception: {ex}");
                MessageBox.Show($"An error occurred during simulation: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            // Signal no more placements will be written; the drain timer is stopped via
            // StopVisualizationTimer in the status transitions below / cancel path.
            queenChannel?.Writer.TryComplete();

            // Tear down the pause gate; the run is over so no loop can be waiting on it.
            IsPaused = false;
            _pauseGate?.Dispose();
            _pauseGate = null;

            if (cancellationToken.IsCancellationRequested)
            {
                IsSimulating = false;
                IsSingleRunning = false;
                ProgressVisibility = Visibility.Collapsed;
                ProgressLabelVisibility = Visibility.Collapsed;
                RefreshCommandStates();
            }
            else
            {
                ManageSimulationStatus(SimulationStatus.Finished);
            }
        }
    }

    private void Save()
    {
        if (ParsingUtils.TryParseInt(BoardSizeText, out _) == false)
            return;

        if (IsOutputReady == false || ObservableSolutions.Count == 0)
            return;

        var filePath = _saveFileService.ShowSaveFileDialog();
        if (string.IsNullOrEmpty(filePath))
            return;

        try
        {
            var content = GenerateSaveContent();
            _saveFileService.SaveContent(filePath, content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Save] Error during save operation: {ex.Message}");
            MessageBox.Show("Failed to save solutions.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel()
    {
        if (IsSimulating == false)
            return;

        // CancellationTokenSource.Cancel() is the single cancellation signal: the solver observes
        // it via the token captured in SimulateAsync (threaded through SimulationContext.Cancellation),
        // and the VM's post-cancel guards read CancellationTokenSource?.IsCancellationRequested.
        try { CancellationTokenSource?.Cancel(); } catch (Exception ex) { Debug.WriteLine($"[Cancel] CTS cancel exception: {ex}"); }

        // Release a paused search so its wait unblocks and the loop can observe cancellation.
        IsPaused = false;
        _pauseGate?.Set();

        StopVisualizationTimer();

        _uiDispatcher.Invoke(() =>
        {
            SelectedSolution = null!;
            ObservableSolutions.Clear();
            ChessboardVm?.ClearImages();
            NoOfSolutions = "0";
            ElapsedTimeInSec = $"{0,0:N1}";
            MemoryConsumption = "0";
            IsOutputReady = false;

            _progressFinalized = false;
            _progressPercent = 0;
            ProgressValue = 0.0;
            ProgressLabel = string.Empty;
            ProgressVisibility = Visibility.Collapsed;
            ProgressLabelVisibility = Visibility.Collapsed;

            IsSimulating = false;
            IsSingleRunning = false;
            IsInInputMode = true;
            IsIdle = true;
        });

        try { CancellationTokenSource?.Dispose(); } catch { }
        CancellationTokenSource = new CancellationTokenSource();
        HandlePostCancel();
    }

    private void ManageSimulationStatus(SimulationStatus simulationStatus)
    {
        switch (simulationStatus)
        {
            case SimulationStatus.Started:
                ResetProgress();
                StopVisualizationTimer();
                IsIdle = false;
                IsInInputMode = false;
                IsSimulating = true;
                IsOutputReady = false;
                IsSingleRunning = SolutionMode == SolutionMode.Single;
                RefreshCommandStates();
                break;

            case SimulationStatus.Finished:
                StopVisualizationTimer();
                IsIdle = true;
                IsInInputMode = true;
                IsSimulating = false;
                IsSingleRunning = false;
                IsOutputReady = true;

                if (!(SolutionMode == SolutionMode.Single && DisplayMode == DisplayMode.Visualize))
                    FinalizeProgressIfApplicable();

                ProgressVisibility = Visibility.Collapsed;
                ProgressLabelVisibility = Visibility.Collapsed;

                bool hideMode = DisplayMode == DisplayMode.Hide;
                bool anyMaterialized = _batchedSolutions.Count > 0;

                _uiDispatcher.Invoke(() =>
                {
                    if (anyMaterialized && ObservableSolutions.Count == 0)
                    {
                        int cap = SimulationSettings.MaxDisplayedCount;
                        foreach (var sol in _batchedSolutions)
                        {
                            if (cap > 0 && ObservableSolutions.Count >= cap) break;
                            ObservableSolutions.Add(sol);
                        }
                    }

                    if (ObservableSolutions.Count > 0)
                    {
                        var first = ObservableSolutions[0];
                        if (!ReferenceEquals(SelectedSolution, first))
                            SelectedSolution = first;

                        if (!hideMode)
                        {
                            EnsureBoardSized();
                            ChessboardVm.PlaceQueens(first.Positions);
                        }
                    }

                    if (SimulationResults != null)
                        NoOfSolutions = NumericUtils.FormatWithSpaceSeparator(SimulationResults.SolutionsCount);
                });

                _batchedSolutions.Clear();
                StopVisualizationTimer();
                RefreshCommandStates();

                // Signal completion only after the final board has been rendered above. Subscribers
                // (and tests awaiting this event) must observe the fully-painted final solution, not
                // a board still mid-animation.
                SimulationCompleted?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    // ----------------- COMMAND CAN-EXECUTE PREDICATES -----------------

    private bool CanSimulate() =>
        IsValid &&
        HasErrors == false &&
        IsSimulating == false &&
        string.IsNullOrWhiteSpace(BoardSizeText) == false;

    private bool CanCancel() => IsSimulating;

    private bool CanSave() =>
        IsOutputReady &&
        ObservableSolutions.Count > 0 &&
        HasErrors == false &&
        string.IsNullOrWhiteSpace(BoardSizeText) == false;

    // ----------------- SUPPORT / HELPERS -----------------

    private void ExtractCorrectNoOfSols()
    {
        ObservableSolutions.Clear();
        if (SimulationResults == null) return;

        var cap = SimulationSettings.MaxDisplayedCount;
        IEnumerable<Solution> sols = SimulationResults.Solutions;
        if (cap > 0 && SimulationResults.SolutionsCount > (ulong)cap)
            sols = sols.Take(cap);

        foreach (var s in sols)
            ObservableSolutions.Add(s);
    }

    private string GenerateSaveContent()
    {
        if (ParsingUtils.TryParseInt(BoardSizeText, out _) == false)
            return string.Empty;

        StringBuilder sb = new();
        sb.AppendLine($"Date && Time: {DateTime.Now}");
        sb.AppendLine($"Board Size: {BoardSizeText}");
        sb.AppendLine($"SolutionMode: {SolutionMode}");
        sb.AppendLine($"Number of Solutions: {NoOfSolutions}");
        sb.AppendLine($"Max Number of Solutions Included: {SimulationSettings.MaxDisplayedCount}");
        sb.AppendLine($"Elapsed Time: {ElapsedTimeInSec} seconds");
        sb.AppendLine($"Memory Usage: {MemoryConsumption} MB");
        sb.AppendLine();
        sb.AppendLine("Solutions:");

        foreach (var solution in ObservableSolutions)
        {
            sb.Append($"Solution ID: {solution.Id}\t");
            sb.AppendLine(solution.Details);
        }

        return sb.ToString();
    }

    private void UpdateUiState()
    {
        if (ParsingUtils.TryParseInt(BoardSizeText, out var boardSize) == false)
            return;

        ObservableSolutions.Clear();
        NoOfSolutions = "0";
        ElapsedTimeInSec = $"{0,0:N1}";
        MemoryConsumption = "0";
        ChessboardVm?.CreateSquares(boardSize);
    }

    private void RefreshCommandStates()
    {
        SimulateCommand?.NotifyCanExecuteChanged();
        CancelCommand?.NotifyCanExecuteChanged();
        SaveCommand?.NotifyCanExecuteChanged();
        TogglePauseCommand?.NotifyCanExecuteChanged();
    }

    // Stop/Resume is available only while an animated Visualized Single (N <= 8) run is active.
    private bool CanTogglePause() =>
        IsSimulating &&
        _pauseGate != null &&
        DisplayMode == DisplayMode.Visualize &&
        SolutionMode == SolutionMode.Single;

    private void TogglePause()
    {
        var gate = _pauseGate;
        if (gate == null) return;

        if (IsPaused)
        {
            gate.Set();      // resume: search continues from where it stopped
            IsPaused = false;
            IsSingleRunning = true;    // restart the indeterminate progress animation
            // Re-establish the delay pacing before resuming: the engine keeps sleeping the fixed
            // delay between placements, so the UI timer must poll at the matching cadence again.
            SyncTimerInterval();
            _visualizeTimer?.Start();  // resume rendering placements
        }
        else
        {
            gate.Reset();    // stop: search blocks, placed queens stay put
            IsPaused = true;
            IsSingleRunning = false;   // freeze the indeterminate progress animation
            _visualizeTimer?.Stop();   // freeze the board immediately at the current frame
        }
    }

    private Solution? _selectedSolution;
    public Solution? SelectedSolution
    {
        get => _selectedSolution;
        set
        {
            if (!SetProperty(ref _selectedSolution, value)) return;
            if (value == null || ChessboardVm == null) return;

            // During a live Visualize run the chessboard is owned by the animation timer
            // (it drains the QueenPlaced channel and renders the search build-up). The solver
            // auto-selects every solution it finds, so stopping the timer / statically painting
            // the board here would halt the animation after the first solution. Keep tracking the
            // selection (SetProperty above) but leave the board to the timer; the final solution is
            // rendered when the run finishes (ManageSimulationStatus.Finished).
            if (IsSimulating && DisplayMode == DisplayMode.Visualize)
                return;

            StopVisualizationTimer();
            var n = value.BoardSize;
            if (ChessboardVm.Squares.Count == 0 || !ChessboardVm.IsBoardStateUpdatedAndSquaresPopulated(n))
                ChessboardVm.CreateSquares(n);
            ChessboardVm.PlaceQueens(value.Positions);
        }
    }
}
