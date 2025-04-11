[![.NET 9 Tests](https://github.com/jamesphenry/UmbralEmpires/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/jamesphenry/UmbralEmpires/actions/workflows/dotnet-desktop.yml)

# UmbralEmpires
>Command the stars at your own pace in a single-player reimagining of AstroEmpires. Build sprawling space stations, colonize distant worlds, amass massive fleets, and outwit ruthless AI empires in a galaxy that evolves with your every move. No timers, no trolls—just you, your strategy, and the infinite void.
---
## Test Results

<!-- TEST-RESULTS-START -->
# Test Run Report (2025-04-11 07:30:30 UTC)

## Summary
* **Overall Outcome:** Failed
* Total Tests: 4
* Passed: 3
* Failed: 1
* Skipped: 0

## Details

### [](#)
| Test Name | Outcome | Duration (ms) | Error Message |
|-----------|---------|---------------|---------------|
| LoadStructures_Should_Load_Single_Simple_Structure_From_Json | Passed | 0 | - |
| LoadStructures_Should_Load_Multiple_Simple_Structures_From_Json | Passed | 0 | - |
| LoadStructures_Should_Throw_Exception_For_Invalid_Json | Failed | 0 | Expected exception message to match the equivalent of "*invalid JSON*", but "'/' is an invalid start of a property name. Expected a '"'. Path: $[0] \| LineNumber: 3 \| BytePositionInLine: 32." does not. |
| LoadStructures_Should_Return_Empty_List_For_Empty_Json_Array | Passed | 0 | - |

<!-- TEST-RESULTS-END -->

