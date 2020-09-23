--
-- Helper table
--
DROP TABLE IF EXISTS Dual;
CREATE TABLE Dual (Dummy  String)
ENGINE = MergeTree()
ORDER BY Dummy
;
INSERT INTO  Dual (Dummy) VALUES ('X');

DROP TABLE IF EXISTS InheritanceParent;
CREATE TABLE InheritanceParent
(
	InheritanceParentId          Int32,
	TypeDiscriminator   Nullable(Int32),
	Name                Nullable(String)
)
ENGINE = MergeTree()
ORDER BY InheritanceParentId
PRIMARY KEY InheritanceParentId;

DROP TABLE IF EXISTS InheritanceChild;
CREATE TABLE InheritanceChild
(
	InheritanceChildId           Int32,
	InheritanceParentId          Int32,
	TypeDiscriminator   Nullable(Int32),
	Name                Nullable(String)
)
ENGINE = MergeTree()
ORDER BY InheritanceChildId
PRIMARY KEY InheritanceChildId;

--
-- Person Table
--
DROP TABLE IF EXISTS Doctor;
DROP TABLE IF EXISTS Patient;
DROP TABLE IF EXISTS Person;
CREATE TABLE Person
(
	PersonID            Int32,
	FirstName           String,
	LastName            String,
	MiddleName Nullable(String),
	Gender              FixedString(1),
	
	CONSTRAINT CK_Person_Gender CHECK (Gender in ('M', 'F', 'U', 'O'))
)
ENGINE = MergeTree()
ORDER BY PersonID
PRIMARY KEY PersonID;

INSERT INTO Person (PersonID, FirstName, LastName, Gender)             VALUES (1, 'John',   'Pupkin',      'M');
INSERT INTO Person (PersonID, FirstName, LastName, Gender)             VALUES (2, 'Tester', 'Testerson',   'M');
INSERT INTO Person (PersonID, FirstName, LastName, Gender)             VALUES (3, 'Jane',   'Doe',         'F');
INSERT INTO Person (PersonID, FirstName, LastName, MiddleName, Gender) VALUES (4, 'Jürgen', 'König', 'Ko', 'M');

--
-- Doctor Table Extension
--
CREATE TABLE Doctor
(
	PersonID Int32,
	Taxonomy String
)
ENGINE = MergeTree()
ORDER BY PersonID
PRIMARY KEY PersonID;

INSERT INTO Doctor (PersonID, Taxonomy) VALUES (1, 'Psychiatry');

--
-- Patient Table Extension
--
CREATE TABLE Patient
(
	PersonID  Int32,
	Diagnosis String
)
ENGINE = MergeTree()
ORDER BY PersonID
PRIMARY KEY PersonID;

INSERT INTO Patient (PersonID, Diagnosis) VALUES (2, 'Hallucination with Paranoid Bugs'' Delirium of Persecution');

--
-- Babylon test
--
DROP TABLE IF EXISTS Parent;
DROP TABLE IF EXISTS Child;
DROP TABLE IF EXISTS GrandChild;

CREATE TABLE Parent      (ParentID Int32, Value1  Nullable(Int32))           ENGINE = MergeTree() ORDER BY ParentID;
CREATE TABLE Child       (ParentID Int32, ChildID Int32)                     ENGINE = MergeTree() ORDER BY ParentID;
CREATE TABLE GrandChild  (ParentID Int32, ChildID Int32, GrandChildID Int32) ENGINE = MergeTree() ORDER BY ParentID;

DROP TABLE IF EXISTS LinqDataTypes;
CREATE TABLE LinqDataTypes
(
	ID                      Int32,
	MoneyValue              Decimal(10,4),
	DateTimeValue  Nullable(DateTime64(3)),
	DateTimeValue2 Nullable(DateTime64(5)),
	BoolValue      Nullable(UInt8),
	GuidValue      Nullable(UUID),
	BinaryValue    Nullable(String),
	SmallIntValue  Nullable(Int16),
	IntValue       Nullable(Int32),
	BigIntValue    Nullable(Int64),
	StringValue    Nullable(String)
)
ENGINE = MergeTree()
ORDER BY ID
PRIMARY KEY ID;



DROP TABLE IF EXISTS TestIdentity
;

CREATE TABLE TestIdentity (
	ID Int32
)
ENGINE = MergeTree()
ORDER BY ID
PRIMARY KEY ID;

DROP TABLE IF EXISTS AllTypes
;

CREATE TABLE AllTypes
(
	ID                                Int32,

	bigintDataType           Nullable(Int64),
	numericDataType          Nullable(decimal(20,0)),
	bitDataType              Nullable(UInt8),
	smallintDataType         Nullable(Int16),
	decimalDataType          Nullable(decimal(20,4)),
	intDataType              Nullable(Int32),
	tinyintDataType          Nullable(Int8),
	moneyDataType            Nullable(decimal(20,2)),
	floatDataType            Nullable(Float32),
	realDataType             Nullable(Float64),

	datetimeDataType         Nullable(datetime),

	charDataType             Nullable(String),
	char20DataType           Nullable(String),
	varcharDataType          Nullable(String),
	textDataType             Nullable(String),
	ncharDataType            Nullable(String),
	nvarcharDataType         Nullable(String),
	ntextDataType            Nullable(String),

	binaryDataType           Nullable(String),
	varbinaryDataType        Nullable(String),
	imageDataType            Nullable(String),

	uniqueidentifierDataType Nullable(UUID),
	objectDataType           Nullable(String)
)
ENGINE = MergeTree()
ORDER BY ID
PRIMARY KEY ID;

INSERT INTO AllTypes
(
	ID,
	bigintDataType, numericDataType, bitDataType, smallintDataType, decimalDataType,
	intDataType, tinyintDataType, moneyDataType, floatDataType, realDataType,
	datetimeDataType,
	charDataType, varcharDataType, textDataType, ncharDataType, nvarcharDataType, ntextDataType,
	objectDataType
)
VALUES
(
		1,
		 NULL,      NULL,  NULL,    NULL,    NULL,   NULL,  NULL,   NULL,  NULL, NULL,
		 NULL,
		 NULL,      NULL,  NULL,    NULL,    NULL,   NULL,
		 NULL
),
(
	2,
	 1000000,    9999999,     1,   25555, 2222222, 7777777,  100, 100000, 20.31, 16.2,
	'2012-12-12 12:12:12',
		  '1',     '234', '567', '23233',  '3323',  '111',
		   '10'

);


--
-- Demonstration Tables for Issue #784
--

-- Parent table
DROP TABLE IF EXISTS PrimaryKeyTable
;

CREATE TABLE PrimaryKeyTable
(
	ID           Int32,
	Name         String
)
ENGINE = MergeTree()
ORDER BY ID
PRIMARY KEY ID;

-- Child table
DROP TABLE IF EXISTS ForeignKeyTable
;

CREATE TABLE ForeignKeyTable
(
	PrimaryKeyTableID Int32,
	Name              String
)
ENGINE = MergeTree()
ORDER BY PrimaryKeyTableID;

-- Second-level child table, alternate semantics
DROP TABLE IF EXISTS FKTestPosition
;

CREATE TABLE FKTestPosition
(
	Company      Int32,
	Department   Int32,
	PositionID   Int32,
	Name         String
)
ENGINE = MergeTree()
ORDER   BY  (Company, Department, PositionID)
PRIMARY KEY (Company, Department, PositionID);

-- merge test tables
DROP TABLE IF EXISTS TestMerge1;
DROP TABLE IF EXISTS TestMerge2;
CREATE TABLE TestMerge1
(
	Id                       Int32,
	Field1          Nullable(Int32),
	Field2          Nullable(Int32),
	Field3          Nullable(Int32),
	Field4          Nullable(Int32),
	Field5          Nullable(Int32),

	FieldInt64      Nullable(Int64),
	FieldBoolean    Nullable(UInt8),
	FieldString     Nullable(String),
	FieldNString    Nullable(String),
	FieldChar       Nullable(FixedString(1)),
	FieldNChar      Nullable(FixedString(1)),
	FieldFloat      Nullable(Float32),
	FieldDouble     Nullable(Float64),
	FieldDateTime   Nullable(DateTime),
	FieldBinary     Nullable(String),
	FieldGuid       Nullable(UUID),
	FieldDate       Nullable(Date),
	FieldEnumString Nullable(String),
	FieldEnumNumber Nullable(Int32)
)
ENGINE = MergeTree()
ORDER BY Id
PRIMARY KEY Id;

CREATE TABLE TestMerge2
(
	Id                       Int32,
	Field1          Nullable(Int32),
	Field2          Nullable(Int32),
	Field3          Nullable(Int32),
	Field4          Nullable(Int32),
	Field5          Nullable(Int32),

	FieldInt64      Nullable(Int64),
	FieldBoolean    Nullable(UInt8),
	FieldString     Nullable(String),
	FieldNString    Nullable(String),
	FieldChar       Nullable(FixedString(1)),
	FieldNChar      Nullable(FixedString(1)),
	FieldFloat      Nullable(Float32),
	FieldDouble     Nullable(Float64),
	FieldDateTime   Nullable(DateTime),
	FieldBinary     Nullable(String),
	FieldGuid       Nullable(UUID),
	FieldDate       Nullable(Date),
	FieldEnumString Nullable(String),
	FieldEnumNumber Nullable(Int32)
)
ENGINE = MergeTree()
ORDER BY Id
PRIMARY KEY Id;

DROP TABLE IF EXISTS TEST_T4_CASING;
CREATE TABLE TEST_T4_CASING
(
	ALL_CAPS              Int32,
	CAPS                  Int32,
	PascalCase            Int32,
	Pascal_Snake_Case     Int32,
	PascalCase_Snake_Case Int32,
	snake_case            Int32,
	camelCase             Int32
)
ENGINE = MergeTree()
ORDER BY ALL_CAPS;

DROP TABLE IF EXISTS FTS3_TABLE;
CREATE TABLE FTS3_TABLE (text1 String, text2 String) ENGINE = MergeTree() ORDER BY text1;

DROP TABLE IF EXISTS FTS4_TABLE;
CREATE TABLE FTS4_TABLE (text1 String, text2 String) ENGINE = MergeTree() ORDER BY text1;

INSERT INTO FTS3_TABLE(text1, text2) VALUES('this is text1', 'this is text2');
INSERT INTO FTS3_TABLE(text1, text2) VALUES('looking for something?', 'found it!');
INSERT INTO FTS3_TABLE(text1, text2) VALUES('record not found', 'empty');
INSERT INTO FTS3_TABLE(text1, text2) VALUES('for snippet testing', 'During 30 Nov-1 Dec, 2-3oC drops. Cool in the upper portion, minimum temperature 14-16oC and cool elsewhere, minimum temperature 17-20oC. Cold to very cold on mountaintops, minimum temperature 6-12oC. Northeasterly winds 15-30 km/hr. After that, temperature increases. Northeasterly winds 15-30 km/hr.');

INSERT INTO FTS4_TABLE(text1, text2) VALUES('this is text1', 'this is text2');
INSERT INTO FTS4_TABLE(text1, text2) VALUES('looking for something?', 'found it!');
INSERT INTO FTS4_TABLE(text1, text2) VALUES('record not found', 'empty');
INSERT INTO FTS4_TABLE(text1, text2) VALUES('for snippet testing', 'During 30 Nov-1 Dec, 2-3oC drops. Cool in the upper portion, minimum temperature 14-16oC and cool elsewhere, minimum temperature 17-20oC. Cold to very cold on mountaintops, minimum temperature 6-12oC. Northeasterly winds 15-30 km/hr. After that, temperature increases. Northeasterly winds 15-30 km/hr.');


