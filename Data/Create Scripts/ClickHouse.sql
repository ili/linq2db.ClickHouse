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
	InheritanceParentId Int32      NOT NULL,
	TypeDiscriminator   Int32          NULL,
	Name                String     NULL
)
ENGINE = MergeTree()
ORDER BY InheritanceParentId
PRIMARY KEY InheritanceParentId;

DROP TABLE IF EXISTS InheritanceChild;
CREATE TABLE InheritanceChild
(
	InheritanceChildId  Int32      NOT NULL,
	InheritanceParentId Int32      NOT NULL,
	TypeDiscriminator   Int32          NULL,
	Name                String     NULL
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
	PersonID   Int32          NOT NULL,
	FirstName  String         NOT NULL,
	LastName   String         NOT NULL,
	MiddleName String             NULL,
	Gender     FixedString(1) NOT NULL,
	
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
	PersonID Int32  NOT NULL,
	Taxonomy String NOT NULL
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
	PersonID  Int32  NOT NULL,
	Diagnosis String NOT NULL
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

CREATE TABLE Parent      (ParentID Int32, Value1  Int32 NULL) ENGINE = MergeTree() ORDER BY ParentID;
CREATE TABLE Child       (ParentID Int32, ChildID Int32)      ENGINE = MergeTree() ORDER BY ParentID;
CREATE TABLE GrandChild  (ParentID Int32, ChildID Int32, GrandChildID Int32) ENGINE = MergeTree() ORDER BY ParentID;

DROP TABLE IF EXISTS LinqDataTypes;
CREATE TABLE LinqDataTypes
(
	ID             Int32,
	MoneyValue     Decimal(10,4),
	DateTimeValue  DateTime64(3) NULL,
	DateTimeValue2 DateTime64(5) NULL,
	BoolValue      UInt8         NULL,
	GuidValue      UUID          NULL,
	BinaryValue    binary(5000)  NULL,
	SmallIntValue  Int16         NULL,
	IntValue       Int32         NULL,
	BigIntValue    Int64         NULL,
	StringValue    String        NULL
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
	ID                       Int32          NOT NULL,

	bigintDataType           Int64            NULL,
	numericDataType          decimal(20,0)    NULL,
	bitDataType              UInt8            NULL,
	smallintDataType         Int16            NULL,
	decimalDataType          decimal(20,4)    NULL,
	intDataType              Int32            NULL,
	tinyintDataType          Int8             NULL,
	moneyDataType            decimal(20,2)    NULL,
	floatDataType            Float32            NULL,
	realDataType             Float64          NULL,

	datetimeDataType         datetime         NULL,

	charDataType             FixedString(1)          NULL,
	char20DataType           FixedString(20)         NULL,
	varcharDataType          String      NULL,
	textDataType             String             NULL,
	ncharDataType            FixedString(20)        NULL,
	nvarcharDataType         String     NULL,
	ntextDataType            String            NULL,

	binaryDataType           binary(5000)           NULL,
	varbinaryDataType        binary(5000)        NULL,
	imageDataType            binary(5000)            NULL,

	uniqueidentifierDataType UUID NULL,
	objectDataType           binary(5000)           NULL
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
	ID           Int32  NOT NULL,
	Name         String NOT NULL
)
ENGINE = MergeTree()
ORDER BY ID
PRIMARY KEY ID;

-- Child table
DROP TABLE IF EXISTS ForeignKeyTable
;

CREATE TABLE ForeignKeyTable
(
	PrimaryKeyTableID Int32  NOT NULL,
	Name              String NOT NULL
)
ENGINE = MergeTree()
ORDER BY PrimaryKeyTableID;

-- Second-level child table, alternate semantics
DROP TABLE IF EXISTS FKTestPosition
;

CREATE TABLE FKTestPosition
(
	Company      Int32      NOT NULL,
	Department   Int32      NOT NULL,
	PositionID   Int32      NOT NULL,
	Name         String     NOT NULL
)
ENGINE = MergeTree()
ORDER BY (Company, Department, PositionID)
PRIMARY KEY (Company, Department, PositionID);

-- merge test tables
DROP TABLE IF EXISTS TestMerge1;
DROP TABLE IF EXISTS TestMerge2;
CREATE TABLE TestMerge1
(
	Id              Int32       NOT NULL,
	Field1          Int32           NULL,
	Field2          Int32           NULL,
	Field3          Int32           NULL,
	Field4          Int32           NULL,
	Field5          Int32           NULL,

	FieldInt64      Int64           NULL,
	FieldBoolean    UInt8           NULL,
	FieldString     String          NULL,
	FieldNString    String          NULL,
	FieldChar       FixedString(1)  NULL,
	FieldNChar      FixedString(1)  NULL,
	FieldFloat      Float32         NULL,
	FieldDouble     Float64         NULL,
	FieldDateTime   DateTime        NULL,
	FieldBinary     binary(20)      NULL,
	FieldGuid       UUID            NULL,
	FieldDate       Date            NULL,
	FieldEnumString String          NULL,
	FieldEnumNumber Int32           NULL
)
ENGINE = MergeTree()
ORDER BY Id
PRIMARY KEY Id;

CREATE TABLE TestMerge2
(
	Id              Int32       NOT NULL,
	Field1          Int32           NULL,
	Field2          Int32           NULL,
	Field3          Int32           NULL,
	Field4          Int32           NULL,
	Field5          Int32           NULL,

	FieldInt64      Int64           NULL,
	FieldBoolean    UInt8           NULL,
	FieldString     String          NULL,
	FieldNString    String          NULL,
	FieldChar       FixedString(1)  NULL,
	FieldNChar      FixedString(1)  NULL,
	FieldFloat      Float32         NULL,
	FieldDouble     Float64         NULL,
	FieldDateTime   DateTime        NULL,
	FieldBinary     binary(20)      NULL,
	FieldGuid       UUID            NULL,
	FieldDate       Date            NULL,
	FieldEnumString String          NULL,
	FieldEnumNumber Int32           NULL
)
ENGINE = MergeTree()
ORDER BY Id
PRIMARY KEY Id;

DROP TABLE IF EXISTS TEST_T4_CASING;
CREATE TABLE TEST_T4_CASING
(
	ALL_CAPS              Int32    NOT NULL,
	CAPS                  Int32    NOT NULL,
	PascalCase            Int32    NOT NULL,
	Pascal_Snake_Case     Int32    NOT NULL,
	PascalCase_Snake_Case Int32    NOT NULL,
	snake_case            Int32    NOT NULL,
	camelCase             Int32    NOT NULL
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


