CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
CREATE TABLE [Role] (
    RoleId INT PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE   )   ;

    CREATE TABLE [User] (
    UserId INT PRIMARY KEY,
    RoleId INT NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    FOREIGN KEY (RoleId) REFERENCES [Role](RoleId)
);

CREATE TABLE [Event] (
    EventId INT PRIMARY KEY,
    OrganiserId INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description VARCHAR(MAX) NULL,
    EventDate DATETIME NOT NULL,
    Location VARCHAR(200) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrganiserId) REFERENCES [User](UserId)
);
CREATE TABLE Category (
    CategoryId INT PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Distance FLOAT NOT NULL,
    EntryFee DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (EventId) REFERENCES [Event](EventId) ON DELETE CASCADE
);

-- 6. Create RouteInformation Table
CREATE TABLE RouteInformation (
    RouteId INT PRIMARY KEY,
    EventId INT NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    StartElevation FLOAT NULL,
    EndElevation FLOAT NULL,
    GPXData VARCHAR(MAX) NULL,
    FOREIGN KEY (EventId) REFERENCES [Event](EventId) ON DELETE CASCADE
);

-- 7. Create Enrolment Table
CREATE TABLE Enrolment (
    EnrolmentId INT PRIMARY KEY,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentStatus VARCHAR(50) NOT NULL DEFAULT 'Unpaid',
    FOREIGN KEY (ParticipantId) REFERENCES [User](UserId),
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId),
    CONSTRAINT UQ_Participant_Category UNIQUE (ParticipantId, CategoryId)
);

-- 8. Create Result Table
CREATE TABLE Result (
    ResultId INT PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE,
    FinishTime VARCHAR(20) NULL,
    OverallPosition INT NULL,
    CategoryPosition INT NULL,
    IsCompleted BIT DEFAULT 1,
    FOREIGN KEY (EnrolmentId) REFERENCES Enrolment(EnrolmentId)
);

INSERT INTO [Role] (RoleId, RoleName) VALUES 
(1, 'Organiser'), 
(2, 'Participant');

-- Seed Users (2 Organisers, 2 Participants)
INSERT INTO [User] (UserId, RoleId, FirstName, LastName, Email, PasswordHash, PhoneNumber) VALUES
(1, 1, 'Sipho', 'Dlamini', 'sipho.dlamini@events.co.za', '$2a$12$e8Jk21K91x31/ExampleHashOrg1', '0821234567'),
(2, 1, 'Anika', 'Van Der Merwe', 'anika@sportsmanagement.co.za', '$2a$12$e8Jk21K91x31/ExampleHashOrg2', '0839876543'),
(3, 2, 'Thabo', 'Mokoena', 'thabo.mokoena@gmail.com', '$2a$12$e8Jk21K91x31/ExampleHashPart1', '0711112222'),
(4, 2, 'Sarah', 'Jenkins', 'sarah.j@yahoo.com', '$2a$12$e8Jk21K91x31/ExampleHashPart2', '0723334444');

-- Seed Events
INSERT INTO [Event] (EventId, OrganiserId, EventName, Description, EventDate, Location) VALUES
(1, 1, 'Soweto Marathon 2026', 'The iconic Peoples Race through historic Soweto.', '2026-11-01 06:00:00', 'FNB Stadium, Johannesburg'),
(2, 1, 'Cape Town Cycle Tour', 'World famous scenic cycling race around the Cape Peninsula.', '2026-10-15 06:30:00', 'Grand Parade, Cape Town'),
(3, 2, 'Two Oceans Marathon', 'The world standard beautiful ultramarathon and half marathon.', '2026-09-20 05:30:00', 'Newlands, Cape Town');

-- Seed Categories
INSERT INTO Category (CategoryId, EventId, CategoryName, Distance, EntryFee) VALUES
(1, 1, '10km Open Run', 10.0, 150.00),
(2, 1, '21km Half Marathon', 21.1, 280.00),
(3, 1, '42km Full Marathon', 42.2, 450.00),
(4, 2, '40km Short Route Cycle', 40.0, 350.00),
(5, 2, '109km Full Route Cycle', 109.0, 680.00),
(6, 3, '21km Half Marathon', 21.1, 320.00),
(7, 3, '56km Ultra Marathon', 56.0, 580.00);

-- Seed Route Information
INSERT INTO RouteInformation (RouteId, EventId, RouteName, StartElevation, EndElevation, GPXData) VALUES
(1, 1, 'Soweto Classic Route', 1650.0, 1650.0, 'https://raceday.co.za/gpx/soweto2026.gpx'),
(2, 2, 'Peninsula Coastal Loop', 15.0, 15.0, 'https://raceday.co.za/gpx/capetowncycle2026.gpx'),
(3, 3, 'Chapmans Peak Climb', 20.0, 20.0, 'https://raceday.co.za/gpx/twooceans2026.gpx');

-- Seed Enrolments
INSERT INTO Enrolment (EnrolmentId, ParticipantId, CategoryId, Status, PaymentStatus) VALUES
(1, 3, 2, 'Confirmed', 'Paid'),
(2, 3, 5, 'Confirmed', 'Paid'),
(3, 4, 1, 'Confirmed', 'Paid'),
(4, 4, 6, 'Pending', 'Unpaid');

-- Seed Results
INSERT INTO Result (ResultId, EnrolmentId, FinishTime, OverallPosition, CategoryPosition, IsCompleted) VALUES
(1, 1, '01:45:30', 142, 28, 1),
(2, 3, '00:52:10', 85, 12, 1);



