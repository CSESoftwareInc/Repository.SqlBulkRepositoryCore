# CSESoftware.Repository.SqlBulkRepositoryCore
### A CSESoftware.Repository.EntityFrameworkCore extensions for Sql Server implementations using .NET Core

---

## Purpose
The purpose of this repository extension is to overcome weaknesses inherent in Entity Framework's handling of large volumes of objects in database operations.

## Create Operations
This repository performs the following steps to streamline create operations:
- Entities passed in are batched to a defined batch size to mitigate SqlBulkCopy's exponential operation time increase past certain benchmarks
- Each batch is written to the database table for the object using SqlBulkCopy
- For Create and Return operations, a single DateTime value is set for all CreatedDate parameters for the objects to be created
	- The CreatedDate parameter is set has ms values removed to overcome Sql weakness in datetime and datetime2 queries
	- The CreatedDate parameter is used to query the database and return all objects for the batches created and return them

A Create operation has the following signature:
await [_name of repository property_].BulkCreateAsync([_list of entities to create_]);

An example would be:
```
var entities = new List<FamilyTree>(); // Some list of entities
await _repository.BulkCreateAsync(entities);
```

A Create and Return operation has the following signature:
var [_name of return property_] = await [_name of repository property_].BulkCreateAndReturnAsync<[_type of entity_], [_Id type for entity_]>([_list of entities to create_]);

An example would be:
```
var entities = new List<FamilyTree>(); // Some list of entities
var returnValues = await _repository.BulkCreateAsync<FamilyTree, Guid>(entities);
```

## Read Operations
This repository performs the following steps to streamline select operations:
- The select parameters passed in are parsed into equivalent Sql datatypes and collected
- The select parameter Sql datatypes are used to create a Temporary Table as part of the current transaction
- Parameters passed in are batched to a defined batch size to mitigate SqlBulkCopy's exponential operation time increase past certain benchmarks
- Each batch is written to the temporary table using SqlBulkCopy
	- A join is performed between the Table for the Entity being selected and the Temporary table on the select parameters
	- All matching values are subjected to an aggregate (optional) secondary query
		- The secondary query can contain Include, OrderBy, Skip, and Take values as well as a Where clause and all will function accordingly
	- The Temporary table has values deleted from it to facilitate additional batches without stacking values
- All batches returned are consolidated and returned as a single list
- The Temporary Table is dropped to ensure it does not persist beyond the transaction

A Read operation has the following signature:
var [_name of return property_] = await [_name of repository property_].BulkSelectAsync(new [_entity to be selected_], [_anonymous list of properties to select by_]);
*Note:* The anonymous list of properties should contain properties where the _property name_ matches that of a property on the entity, the _property type_ matches the type of that property on the entity, and the _property value_ matches what you wish to select by.

An example would be:
```
var genders = new List<string>(); // Some list of strings to find
var selectValues = genders.Select(x => new { Gender = x }).ToList();
var returnValues = await _repository.BulkSelectAsync(new FamilyTree(), selectValues);
```

## Update Operations
This repository performs the following steps to streamline update operations:
- The update parameters passed in are parsed into equivalent Sql datatypes and collected
- The update parameter Sql datatypes are used to create a Temporary Table as part of the current transaction
- Parameters passed in are batched to a defined batch size to mitigate SqlBulkCopy's exponential operation time increase past certain benchmarks
- Each batch is written to the temporary table using SqlBulkCopy
	- A join is performed between the Table for the Entity being selected and the Temporary table on the Id field for the Entity
	- An Update is performed to the Table for the Entity based on the Id value on the Temporary table and updating all other update parameters included
	- The Temporary table has values deleted from it to facilitate additional batches without stacking values
- The Temporary Table is dropped to ensure it does not persist beyond the transaction

An Update operation has the following signature:
await [_name of repository property_].BulkUpdateAsync(new [_entity to be updated_], [_anonymous list of properties to update by_]);
*Note:* The anonymous list of properties must contain the Id field of the entity. Additionally, it should contain properties where the _property name_ matches that of a property on the entity, the _property type_ matches the type of that property on the entity, and the _property value_ matches what you wish to select by.

An example would be
```
var entities = new List<FamilyTree>(); // Some list of entities
var updateValues = entities.Select(x => new { x.Id, Gender = "Banana" }).ToList();
await _repository.BulkUpdateAsync(new FamilyTree(), updateValues);
```

## Delete Operations
This repository performs the following steps to streamline delete operations:
- The delete parameters passed in are parsed into equivalent Sql datatypes and collected
- The delete parameter Sql datatypes are used to create a Temporary Table as part of the current transaction
- Parameters passed in are batched to a defined batch size to mitigate SqlBulkCopy's exponential operation time increase past certain benchmarks
- Each batch is written to the temporary table using SqlBulkCopy
	- A join is performed between the Table for the Entity being selected and the Temporary table on the delete parameters
	- All matching values are deleted from the Table for the Entity
	- The Temporary table has values deleted from it to facilitate additional batches without stacking values
- The Temporary Table is dropped to ensure it does not persist beyond the transaction

A Delete operation has the following signature:
await [_name of repository property_].BulkDeleteAsync(new [_entity to be deleted_], [_anonymous list of properties to delete by_]);
*Note:* The anonymous list of properties should contain properties where the _property name_ matches that of a property on the entity, the _property type_ matches the type of that property on the entity, and the _property value_ matches what you wish to select by.

An example would be:
```
var fatherIds = new List<Guid>(); // Some list of fathers to delete children from
var deleteValues = fatherIds.Select(x => new { FatherId = x }).ToList();
await _repository.BulkDeleteAsync(new FamilyTree(), deleteValues);
```

---

## Known Weaknesses
There are some weaknesses in this Bulk Repository that we are aware of. Future iterations may resolve these weaknesses:
- Multiple bulk Create and Return operations occurring within the same second of time can cause greater numbers of values to return then were written with that specific operation
- BaseEntity<T> must be extended for any Entity to be able to be returned from the bulk Create and Return operation
- Select (IQueryWithSelect) cannot be used in conjunction with bulk Read operations at this time
- The Bulk Repository does not currently support additional functionality for Implicit M:M relationships
- The Bulk Repository can cause deadlocks if dozens of operations are fired at once. A retry policy has been instituted with a hard reset count of 3, but future implementations should seek a more maintainable solution for conquering the deadlocks.

## Tools Used
- SqlBulkCopy
- Fastmember (https://github.com/mgravell/fast-member)
- XUnit for testing

## Test Setup
If you wish to run the Unit Tests for this package, a Sql Server instance is required. The connection string is located in the TestContextFactory and can be pointed at any local server and database desired. Migration Script included in the .TestDatabase project should be run against the intended database to bring it up to the correct migration context for testing.

---

CSE Software, Inc. is a privately held company founded in 1990. CSE develops software, AR/VR, simulation, mobile, and web technology solutions. The company also offers live, 24x7, global help desk services in 110 languages. All CSE teams are U.S. based with experience in multiple industries, including government, military, healthcare, construction, agriculture, mining, and more. CSE Software is a certified women-owned small business. Visit us online at [csesoftware.com](https://www.csesoftware.com).