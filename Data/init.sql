PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL CHECK (length(trim(Name)) BETWEEN 2 AND 80)
);

CREATE TABLE IF NOT EXISTS Resources (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryId INTEGER NOT NULL,
    Code TEXT NOT NULL UNIQUE CHECK (length(Code) BETWEEN 3 AND 20),
    Name TEXT NOT NULL CHECK (length(trim(Name)) BETWEEN 2 AND 120),
    PriceMinor INTEGER NOT NULL CHECK (PriceMinor BETWEEN 0 AND 1000000000),
    Capacity INTEGER NOT NULL CHECK (Capacity BETWEEN 0 AND 100000),
    Available INTEGER NOT NULL CHECK (Available BETWEEN 0 AND Capacity),
    Sku TEXT NOT NULL UNIQUE CHECK (length(trim(Sku)) BETWEEN 3 AND 30),
    Brand TEXT NOT NULL CHECK (length(trim(Brand)) BETWEEN 1 AND 60),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Requests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ResourceId INTEGER NOT NULL,
    CustomerName TEXT NOT NULL CHECK (length(trim(CustomerName)) BETWEEN 2 AND 80),
    Quantity INTEGER NOT NULL CHECK (Quantity > 0 AND Quantity <= 100000),
    UnitPriceMinor INTEGER NOT NULL CHECK (UnitPriceMinor >= 0),
    CreatedAtUtc TEXT NOT NULL,
    Status TEXT NOT NULL CHECK (Status IN ('Active', 'Cancelled')),
    FOREIGN KEY (ResourceId) REFERENCES Resources(Id) ON DELETE RESTRICT
);