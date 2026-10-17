# Tests

Use a test project when it exercises a distinct production responsibility. Organize files and namespaces to mirror the production project and folder, adding `.Tests` (for example, `Seances.Data.Storage.Tests`). Keep deterministic unit tests isolated from host tests; place host-level integration tests under the Web test project's `Integration` folder. Use synthetic fixtures and avoid external network dependencies.
