# 05-seal-classes: Seal non-inherited classes

Mark leaf classes `sealed` (GUI converters, dispatchers, services, leaf ViewModels, test mocks, benchmark classes) after verifying no subclasses or mocking requirements.

**Done when**: Leaf classes sealed; build clean, tests pass; CHANGELOG updated under [Unreleased].
