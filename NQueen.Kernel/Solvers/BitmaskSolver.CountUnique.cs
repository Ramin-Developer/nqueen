namespace NQueen.Kernel.Solvers;

public partial class BitmaskSolver
{
    // Selects the fastest unique count-only algorithm for the given board size.
    // Only called for N <= LookupThresholdN-1 (20); the lookup table makes N>=21 unreachable.
    private ulong CountUniqueAdaptive(int n)
    {
        bool origPrefix = EnablePrefixMinimalityPruning;
        bool origReflection = EnablePartialReflectionPruning;
        EnablePrefixMinimalityPruning = true;
        EnablePartialReflectionPruning = true;

        EnsureMinThreads();

        try
        {
            if (n >= SimulationSettings.UniqueCountOnlyParallelThresholdN)
            {
                // N = 8..20: symmetry-class (Takaken) parallel counter.
                return CountUniqueSymmetryClass(n);
            }
            else
            {
                // N < 8: parallel canonical enumeration via BitmaskParallelEngine.
                ulong total = 0;
                BitmaskParallelEngine.RunUnique(new BitmaskParallelEngine.UniqueRequest
                {
                    BoardSize = n,
                    EnableEvents = false,
                    ShouldMaterialize = () => false,
                    OnUniqueSolution = _ => { },
                    OnCompletedUniqueCount = count => total = count,
                    ReportProgress = _ => { }
                });
                return total;
            }
        }
        finally
        {
            EnablePrefixMinimalityPruning = origPrefix;
            EnablePartialReflectionPruning = origReflection;
        }
    }
}
