# Collection tests

- Keep every unit test independent of external websites and network availability. Test parsing with synthetic HTML and test HTTP workflows with scripted handlers and fixed time providers.
- Never make live UGC or other website requests from unit tests. A source change should be represented by an explicit synthetic fixture or scripted response.
- Keep parser tests focused on observable input and output, including malformed or incomplete source data.
