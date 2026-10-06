namespace NQueen.UnitTests.Tests.Kernel;

[Trait("Category", "Kernel")]
public class BitmaskEngineDirectTests
{
    private static BitmaskSearchEngine.Request CreateRequest(
        int n,
        bool countOnly,
        Func<int[], bool> onSolution,
        bool restrictFirstCol = false,
        bool enhanced = false,
        bool aggressive = false,
        bool prefix = false,
        bool reflection = false,
        Func<bool>? isCanceled = null) =>
        new(n, restrictFirstCol, enhanced, aggressive, countOnly, DisplayMode.Hide, 0,
            isCanceled ?? (() => false), _ => { }, _ => { }, onSolution, prefix, reflection);

    private static bool IsValid(int[] rows)
    {
        for (int i = 0; i < rows.Length; i++)
            for (int j = i + 1; j < rows.Length; j++)
                if (rows[i] == rows[j] || Math.Abs(rows[i] - rows[j]) == j - i)
                    return false;
        return true;
    }

    [Theory]
    [InlineData(6, 4)]
    [InlineData(8, 92)]
    [InlineData(10, 724)]
    public void SearchEngine_CountOnly_FindsAllSolutions(int n, int expected)
    {
        int count = 0;

        BitmaskSearchEngine.Run(CreateRequest(n, countOnly: true, _ => { count++; return false; }));

        count.ShouldBe(expected);
    }

    [Fact]
    public void SearchEngine_CountOnly_StopsWhenCallbackReturnsTrue()
    {
        int count = 0;

        BitmaskSearchEngine.Run(CreateRequest(8, countOnly: true, _ => ++count == 3));

        count.ShouldBe(3);
    }

    [Fact]
    public void SearchEngine_CountOnly_CanceledRunFindsNothing()
    {
        int count = 0;

        BitmaskSearchEngine.Run(CreateRequest(8, countOnly: true, _ => { count++; return false; }, isCanceled: () => true));

        count.ShouldBe(0);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SearchEngine_CountOnlyWithPruning_EmitsOnlyValidSolutions(bool prefix, bool reflection)
    {
        bool allValid = true;
        int count = 0;

        BitmaskSearchEngine.Run(CreateRequest(15, countOnly: true,
            rows => { allValid &= IsValid(rows); count++; return false; },
            restrictFirstCol: true, prefix: prefix, reflection: reflection));

        (allValid && count > 0).ShouldBeTrue();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void SearchEngine_SymmetryPruning_EmitsOnlyValidSolutions(bool enhanced, bool aggressive)
    {
        bool allValid = true;
        int count = 0;

        BitmaskSearchEngine.Run(CreateRequest(14, countOnly: false,
            rows => { allValid &= IsValid(rows); count++; return false; },
            enhanced: enhanced, aggressive: aggressive));

        (allValid && count > 0).ShouldBeTrue();
    }

    [Fact]
    public void SearchEngine_BoardLargerThanMax_Throws() =>
        Should.Throw<NotSupportedException>(() =>
            BitmaskSearchEngine.Run(CreateRequest(BoardSettings.MaxBitmaskBoardSize + 1, countOnly: true, _ => false)));

    [Theory]
    [InlineData(6, 1UL)]
    [InlineData(8, 12UL)]
    [InlineData(10, 92UL)]
    public void ParallelEngine_RunUniqueMaterialize_ReportsCanonicalCount(int n, ulong expected)
    {
        ulong count = 0;

        BitmaskParallelEngine.RunUnique(new BitmaskParallelEngine.UniqueRequest(
            n, EnableEvents: true, ShouldMaterialize: () => true, OnUniqueSolution: _ => { },
            OnCompletedUniqueCount: c => count = c, ReportProgress: _ => { }));

        count.ShouldBe(expected);
    }

    [Fact]
    public void ParallelEngine_RunUniqueMaterialize_CapsEmittedSolutions()
    {
        int emitted = 0;

        BitmaskParallelEngine.RunUnique(new BitmaskParallelEngine.UniqueRequest(
            10, EnableEvents: false, ShouldMaterialize: () => true,
            OnUniqueSolution: _ => Interlocked.Increment(ref emitted),
            OnCompletedUniqueCount: _ => { }, ReportProgress: _ => { }));

        emitted.ShouldBe(SimulationSettings.MaxDisplayedCount);
    }

    [Fact]
    public void ParallelEngine_RunUniqueZeroBoard_ReportsZero()
    {
        ulong count = ulong.MaxValue;

        BitmaskParallelEngine.RunUnique(new BitmaskParallelEngine.UniqueRequest(
            0, EnableEvents: false, ShouldMaterialize: () => false, OnUniqueSolution: _ => { },
            OnCompletedUniqueCount: c => count = c, ReportProgress: _ => { }));

        count.ShouldBe(0UL);
    }
}
