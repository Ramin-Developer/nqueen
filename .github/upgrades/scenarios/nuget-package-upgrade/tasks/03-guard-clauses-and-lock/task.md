# 03-guard-clauses-and-lock: Modernize null guards and synchronization primitive

Use `ArgumentNullException.ThrowIfNull` for constructor/method null checks (MainViewModel, MainWindow, LazyPositionList, SynchronousProgress, BitmaskSolver). Switch `BitmaskSolver._sync` to `System.Threading.Lock` if it is a plain `object`.

**Done when**: Guards modernized without behavior change; build clean, tests pass.
