[![.NET 9 Tests](https://github.com/jamesphenry/UmbralEmpires/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/jamesphenry/UmbralEmpires/actions/workflows/dotnet-desktop.yml)

# UmbralEmpires
>Command the stars at your own pace in a single-player reimagining of AstroEmpires. Build sprawling space stations, colonize distant worlds, amass massive fleets, and outwit ruthless AI empires in a galaxy that evolves with your every move. No timers, no trolls—just you, your strategy, and the infinite void.
---
<!-- TEST-RESULTS-START -->

## Details

### [](#)
**Total: 79 | ✅ Passed: 64 | ❌ Failed: 15 | ⏭ Skipped: 0**
<details><summary>Click to expand test details</summary>

| Test Name | Outcome | Duration (ms) | Error Message |
|-----------|---------|---------------|----------------|
| LoadAllDefinitions_Should_Load_UsesSolar_Flag | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_Single_Simple_Structure | Passed | 0 | - |
| Should_Skip_Technology_With_Duplicate_Prerequisite_TechIds | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_AddsPopCapacityByFertility_Flag | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_IncreasesAstroFertility_Flag | Passed | 0 | - |
| Should_Skip_Defense_With_Invalid_RequiredTechnology_TechId | Passed | 0 | - |
| Should_Skip_Unit_When_RequiredTechnology_Is_Present_But_Invalid | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_AreaRequirementPerLevel | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_UsesMetal_Flag | Passed | 0 | - |
| Should_Return_Empty_Lists_For_Empty_Input_Json | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_UsesGas_Flag | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_BaseConstructionBonus | Passed | 0 | - |
| Should_Load_Unit_When_Definition_Id_Has_Mixed_Case_And_Requirement_Matches_Case | Passed | 0 | - |
| Should_Throw_Exception_For_Invalid_Json | Passed | 0 | - |
| Should_Skip_Defense_With_Invalid_RequiredTechnology_Level | Passed | 0 | - |
| Should_Skip_Unit_With_Negative_Hangar | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_BaseResearchBonus | Passed | 0 | - |
| Should_Load_Unit_Description | Failed | 0 | Expected result.Units to contain a single item, but the collection is empty. |
| Should_Skip_Defense_With_Negative_AreaRequirementPerLevel | Passed | 0 | - |
| Should_Skip_Unit_With_Negative_Speed | Passed | 0 | - |
| Should_Load_Single_Simple_Technology | Passed | 0 | - |
| Should_Skip_Defense_With_Missing_WeaponType | Passed | 0 | - |
| Should_Skip_Structure_When_RequiredTechnology_Is_Missing | Passed | 0 | - |
| LoadAllDefinitions_Should_Skip_Object_With_Missing_Name | Passed | 0 | - |
| Should_Skip_Unit_With_Negative_Cost | Passed | 0 | - |
| Should_Skip_Unit_With_Unknown_DriveType | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Load_Technology_With_Prerequisites | Failed | 0 | Expected resultList to contain a single item, but the collection is empty. |
| LoadAllDefinitions_Should_Load_UsesCrystal_Flag | Passed | 0 | - |
| Should_Skip_Unit_With_Missing_Id | Passed | 0 | - |
| Should_Skip_Unit_With_Negative_RequiredShipyard_BaseLevel | Passed | 0 | - |
| Should_Skip_Defense_With_Negative_EnergyRequirementPerLevel | Passed | 0 | - |
| Should_Skip_Unit_With_Negative_Shield | Passed | 0 | - |
| Should_Skip_Unit_With_Negative_Attack | Passed | 0 | - |
| Should_Skip_Technology_With_Invalid_Prerequisite_TechId | Passed | 0 | - |
| LoadAllDefinitions_Should_Skip_Object_With_Missing_Id | Passed | 0 | - |
| Should_Skip_Technology_With_Negative_Cost | Passed | 0 | - |
| Should_Skip_Defense_With_Null_Entry_In_RequiresTechnology | Passed | 0 | - |
| Should_Ignore_Extra_Json_Properties | Passed | 0 | - |
| Should_Skip_Unit_With_Missing_DriveType | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Load_Single_Simple_Defense | Failed | 0 | Expected resultList to contain a single item, but the collection is empty. |
| Should_Skip_Unit_With_Missing_Name | Passed | 0 | - |
| Should_Skip_Technology_With_Null_Entry_In_Prerequisites_List | Passed | 0 | - |
| Should_Load_Technology_Description | Passed | 0 | - |
| Should_Skip_Defense_With_Negative_Cost | Passed | 0 | - |
| Should_Skip_Technology_With_Negative_LabsLevel | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_BaseProductionBonus | Passed | 0 | - |
| Should_Load_Multiple_Simple_Defenses | Passed | 0 | - |
| Should_Load_Unit_When_RequiredTechnology_Is_Present_And_Valid | Passed | 0 | - |
| Should_Skip_Defense_With_Negative_Attack | Passed | 0 | - |
| Should_Skip_Defense_With_Duplicate_RequiredTechnology_TechIds | Passed | 0 | - |
| Should_Skip_Stellar_Unit_Without_StellarDrive_Tech | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Skip_Unit_With_Negative_Armour | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_AreaCapacityBonus | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_IsAdvanced_Flag | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_RequiresTechnology_List | Failed | 0 | Expected resultList to contain a single item, but the collection is empty. |
| Should_Skip_Unit_With_Invalid_RequiredTechnology_TechId | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| LoadAllDefinitions_Should_Load_Multiple_Simple_Structures | Passed | 0 | - |
| Should_Skip_Technology_With_Invalid_Prerequisite_Level | Passed | 0 | - |
| Should_Skip_Unit_With_Duplicate_RequiredTechnology_TechIds | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Skip_Technology_When_Prerequisite_Is_Missing | Passed | 0 | - |
| LoadAllDefinitions_Should_Skip_Object_With_Negative_Cost | Passed | 0 | - |
| Should_Load_Unit_When_RequiredTechnology_Case_Differs_But_Exists | Passed | 0 | - |
| Should_Skip_Unit_When_RequiredTechnology_Is_Missing | Passed | 0 | - |
| Should_Skip_Unit_With_Missing_WeaponType | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Load_Unit_When_Definition_Id_Has_Mixed_Case_And_Requirement_Uses_Different_Case | Passed | 0 | - |
| Should_Skip_Defense_With_Missing_Name | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_EnergyRequirementPerLevel | Passed | 0 | - |
| Should_Load_Single_Simple_Unit | Failed | 0 | Expected resultList to contain a single item, but the collection is empty. |
| Should_Skip_Defense_With_Negative_Armour | Passed | 0 | - |
| Should_Skip_Defense_With_Negative_Shield | Passed | 0 | - |
| Should_Skip_Unit_With_Invalid_RequiredTechnology_Level | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Skip_Defense_With_Negative_PopulationRequirementPerLevel | Passed | 0 | - |
| Should_Skip_Defense_With_Missing_Id | Passed | 0 | - |
| Should_Skip_Warp_Unit_Without_WarpDrive_Tech | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Load_Unit_When_RequiredTechnology_Case_Differs_From_Definition | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Be strict about the order of items in byte arrays<br>- Without automatic conversion. |
| LoadAllDefinitions_Should_Load_EconomyBonus | Passed | 0 | - |
| Should_Skip_Unit_With_Null_Entry_In_RequiresTechnology | Failed | 0 | Expected result.Units to be a collection with 1 item(s), but found an empty collection.<br><br>With configuration:<br>- Prefer the declared type of the members<br>- Compare enums by value<br>- Compare tuples by their properties<br>- Compare anonymous types by their properties<br>- Compare records by their members<br>- Include non-browsable members<br>- Include all non-private properties<br>- Include all non-private fields<br>- Match member by name (or throw)<br>- Always be strict about the collection order<br>- Without automatic conversion. |
| Should_Skip_Unit_With_Negative_RequiredShipyard_OrbitalLevel | Passed | 0 | - |
| LoadAllDefinitions_Should_Load_PopulationRequirementPerLevel | Passed | 0 | - |

</details>

<!-- TEST-RESULTS-END -->
---

