namespace NQueen.GUI.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _boardSizeText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressLabel))]
    private double _progressValue = 0;

    // Disable DisplayMode and Delay editing while simulating
    public bool CanEditDisplayMode => IsInInputMode && !IsSimulating;
    public bool CanEditDelay => IsInInputMode && !IsSimulating;

    // The delay slider is only meaningful in Visualize mode and must stay fixed at the value
    // decided before the run starts, because changing it mid-simulation has little effect on the
    // already-running engine. Keep it enabled only when visualizing and not simulating.
    public bool CanEditDelaySlider => DisplayMode == DisplayMode.Visualize && !IsSimulating;

    [ObservableProperty]
    private string _progressLabel = string.Empty;

    [ObservableProperty]
    private Visibility _progressVisibility = Visibility.Collapsed;

    partial void OnProgressVisibilityChanged(Visibility value) =>
        IsProgressBarOffscreen = value != Visibility.Visible;

    [ObservableProperty]
    private Visibility _progressLabelVisibility = Visibility.Collapsed;

    partial void OnProgressLabelVisibilityChanged(Visibility value) =>
        IsProgressLabelOffscreen = value != Visibility.Visible;

    [ObservableProperty]
    private bool _isProgressBarOffscreen;

    [ObservableProperty]
    private bool _isProgressLabelOffscreen;

    [ObservableProperty]
    private IEnumerable<SolutionMode> _enumSolutionModes =
        Enum.GetValues<SolutionMode>().Cast<SolutionMode>();

    [ObservableProperty]
    private IEnumerable<DisplayMode> _enumDisplayModes =
        Enum.GetValues<DisplayMode>().Cast<DisplayMode>();

    [ObservableProperty]
    private IEnumerable<ResultStorageMode> _enumStorageModes =
        Enum.GetValues<ResultStorageMode>().Cast<ResultStorageMode>();

    private ResultStorageMode _allStorageMode = SimulationSettings.DefaultAllStorageMode;
    private ResultStorageMode _uniqueStorageMode = SimulationSettings.DefaultUniqueStorageMode;

    public ResultStorageMode SelectedStorageMode
    {
        get
        {
            if (IsVisualized) return ResultStorageMode.Materialize;
            return SolutionMode == SolutionMode.Unique ? _uniqueStorageMode : _allStorageMode;
        }
        set
        {
            if (IsVisualized) return;
            var changed = false;
            switch (SolutionMode)
            {
                case SolutionMode.All:
                case SolutionMode.Single:
                    if (_allStorageMode != value) { _allStorageMode = value; changed = true; }
                    break;
                case SolutionMode.Unique:
                    if (_uniqueStorageMode != value) { _uniqueStorageMode = value; changed = true; }
                    break;
            }
            if (changed)
            {
                OnPropertyChanged();
                ApplyStorageModesToSolver();
            }
        }
    }

    private void ApplyStorageModesToSolver()
    {
        if (_solver is NQueen.Kernel.Solvers.BitmaskSolver b)
        {
            b.AllStorageMode = _allStorageMode;
            b.UniqueStorageMode = _uniqueStorageMode;
        }
    }

    [ObservableProperty]
    private bool _isVisualized;

    [ObservableProperty]
    private int _delayInMilliseconds; // Implementation of OnDelayInMillisecondsChanged moved to Events partial to avoid duplicate

    [ObservableProperty]
    private SimulationResults _simulationResults = new([], 0.0);

    [ObservableProperty]
    private ObservableCollection<Solution> _observableSolutions = [];

    [ObservableProperty]
    private SolutionMode _solutionMode;

    [ObservableProperty]
    private DisplayMode _displayMode;

    [ObservableProperty]
    private bool _isValid = false;

    [ObservableProperty]
    private string _solutionTitle = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResultLabel))]
    private string _noOfSolutions = "0";

    [ObservableProperty]
    private string _memoryConsumption = "0";

    [ObservableProperty]
    private string _elapsedTimeInSec = string.Empty;

    [ObservableProperty]
    private bool _isSimulating;

    partial void OnIsSimulatingChanged(bool value)
    {
        RefreshCommandStates();
        OnPropertyChanged(nameof(CanEditDisplayMode));
        OnPropertyChanged(nameof(CanEditDelay));
        OnPropertyChanged(nameof(CanEditDelaySlider));
    }

    [ObservableProperty]
    private bool _isInInputMode;

    partial void OnIsInInputModeChanged(bool value)
    {
        RefreshCommandStates();
        OnPropertyChanged(nameof(CanChangeStorageMode));
        OnPropertyChanged(nameof(CanEditDisplayMode));
        OnPropertyChanged(nameof(CanEditDelay));
    }

    [ObservableProperty]
    private bool _isSingleRunning;

    [ObservableProperty]
    private bool _isIdle;

    partial void OnIsIdleChanged(bool value) =>
        RefreshCommandStates();

    [ObservableProperty]
    private bool _isOutputReady;

    partial void OnIsOutputReadyChanged(bool value) =>
        RefreshCommandStates();

    [ObservableProperty]
    private bool _suppressUserDialogs;

    [ObservableProperty]
    private bool _useParallel = SimulationSettings.DefaultUseParallel;
    partial void OnUseParallelChanged(bool value)
    {
        if (_solver is NQueen.Kernel.Solvers.BitmaskSolver b)
            b.UseParallel = value;
    }

    [ObservableProperty]
    private int _parallelRootSplitDepth = SimulationSettings.DefaultParallelRootSplitDepth;
    partial void OnParallelRootSplitDepthChanged(int value)
    {
        if (value < 1) ParallelRootSplitDepth = 1;
        else if (_solver is NQueen.Kernel.Solvers.BitmaskSolver b)
            b.ParallelRootSplitDepth = value;
    }

    private void AutoAdjustParallel()
    {
        if (_solver is not NQueen.Kernel.Solvers.BitmaskSolver bs)
            return;
        if (!ParsingUtils.TryParseInt(BoardSizeText, out var n))
            return;

        bool parallel = NQueen.Kernel.Solvers.BitmaskSolverRunConfigurator.ComputeUseParallel(
            n, SolutionMode, DisplayMode);

        bs.UseParallel = parallel;
        UseParallel = parallel;

        int depth = NQueen.Kernel.Solvers.BitmaskSolverRunConfigurator.ComputeParallelRootSplitDepth(
            n, parallel);
        bs.ParallelRootSplitDepth = depth;
        ParallelRootSplitDepth = depth;
        bs.EnableHalfBoardRestriction = ComputeHalfBoardRestriction();
        OnPropertyChanged(nameof(EnableHalfBoardRestriction));
    }

    private bool ComputeHalfBoardRestriction()
    {
        if (!ParsingUtils.TryParseInt(BoardSizeText, out var n)) return false;
        return NQueen.Kernel.Solvers.BitmaskSolverRunConfigurator.ComputeHalfBoardRestriction(
            n, SolutionMode, DisplayMode);
    }

    public bool EnableHalfBoardRestriction
    {
        get => ComputeHalfBoardRestriction();
        set
        {
            var auto = ComputeHalfBoardRestriction();
            if (_solver is NQueen.Kernel.Solvers.BitmaskSolver b)
                b.EnableHalfBoardRestriction = auto;
            OnPropertyChanged();
        }
    }

    public bool CanChangeStorageMode => !IsVisualized && IsInInputMode && SolutionMode != SolutionMode.Single;

    // --- Stop/Resume pause (Visualized Single mode, N <= 8 only) ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseButtonLabel))]
    private bool _isPaused;

    public string PauseButtonLabel => IsPaused ? "Resume" : "Stop";

    // The button exists only for the Visualized Single mode with N <= 8; hidden otherwise.
    public Visibility PauseButtonVisibility =>
        DisplayMode == DisplayMode.Visualize &&
        SolutionMode == SolutionMode.Single &&
        BoardSize <= SimulationSettings.MaxVisualizeSingleBoardSize
            ? Visibility.Visible
            : Visibility.Collapsed;
}
