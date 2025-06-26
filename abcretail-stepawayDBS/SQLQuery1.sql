--Create Database
Create DATABASE abcretailDB;
USE abcretailDB;
-- Create Customer Table

CREATE TABLE Roles(
  RoleID INT PRIMARY KEY IDENTITY,
  RoleName NVARCHAR(50)
);

CREATE TABLE User_s (
    UserID INT PRIMARY KEY IDENTITY,
	RoleID INT FOREIGN KEY REFERENCES Roles(RoleID),
    Username NVARCHAR(50),
    Email NVARCHAR(100) UNIQUE,
    PasswordH NVARCHAR(255),
    PhoneNumber NVARCHAR(15),
    CreatedDate DATETIME DEFAULT GETDATE()
);
select * from Roles;

-- Create Product Table
CREATE TABLE Product (
    ProductID INT PRIMARY KEY IDENTITY,
    ProductName NVARCHAR(100),
    Brand NVARCHAR(255),
	Colour NVARCHAR(255),
	Size NVARCHAR(255),
    Price DECIMAL(10, 2),
    Stock INT,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Create Order Table
CREATE TABLE Orde_r (
    OrderID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES User_s(UserID),
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50),
    ShippingAddress NVARCHAR(255),
    TotalAmount DECIMAL(10, 2),
	TrackingNumber VARCHAR(20)
);

-- Create OrderItem Table (to represent items within each order)
CREATE TABLE OrderItem (
    OrderItemID INT PRIMARY KEY IDENTITY,
    OrderID INT FOREIGN KEY REFERENCES Orde_r(OrderID),
    ProductID INT FOREIGN KEY REFERENCES Product(ProductID),
    Quantity INT,
    Price DECIMAL(10, 2)
);

-- Create Cart Table
CREATE TABLE Cart (
    CartID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES User_s(UserID),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Create CartItem Table (to represent items within each cart)
CREATE TABLE CartItem (
    CartItemID INT PRIMARY KEY IDENTITY,
    CartID INT FOREIGN KEY REFERENCES Cart(CartID),
    ProductID INT FOREIGN KEY REFERENCES Product(ProductID),
    Quantity INT,
    Price DECIMAL(10, 2)
);

-- Create ReturnRequest Table
CREATE TABLE ReturnRequest (
    ReturnRequestID INT PRIMARY KEY IDENTITY,
    OrderID INT FOREIGN KEY REFERENCES Orde_r(OrderID),
    ProductID INT FOREIGN KEY REFERENCES Product(ProductID),
    Reason NVARCHAR(255),
    Status NVARCHAR(50),
    RequestDate DATETIME DEFAULT GETDATE()
);

-- Create Messages Table
CREATE TABLE Messages (
    MessageID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES User_s(UserID),
    MessageText NVARCHAR(500),
    SentDate DATETIME DEFAULT GETDATE(),
);

-- Create Notifications Table
CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY IDENTITY,
    UserID INT FOREIGN KEY REFERENCES User_s(UserID),
    NotificationText NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE(),
);

-- Sample Insert Statements

-- Insert sample customers
INSERT INTO Roles(RoleName)
VALUES 
('Customer'),
('Admin');

INSERT INTO User_s (RoleID,Username, Email, PasswordH, PhoneNumber, CreatedDate)
VALUES 
(1, 'Emily Davis', 'emilydavis@gmail.com', 'emily123', '078-999-0000', '2024-11-11'),
(2, 'Jane Smith', 'janesmith@gmail.com', 'janes123', '098-765-4321', '2024-11-11'),
(1, 'Alice Johnson', 'alice.johnson@example.com', 'aliceJohnson@90', '079-012-3456', '2024-11-11'),
(1, 'Bob Brown', 'bob.brown@gmail.com', 'bob.brown@example.com', '076-789-0123',  '2024-11-11'),
(1, 'JohnDoe', 'johndoe@gmail.com', 'johndoe677', '072-890-8876', '2024-11-11');
 select * from Product;

-- Insert sample products
INSERT INTO Product (ProductName, Brand, Colour, Size, Price, Stock, CreatedDate)
VALUES
('New Balance 530 Cream Beige', 'New Balance', 'Cream Beige', 5, 1999.99, 10, '2024-11-11'),
('New Balance 9060 Steel Blue', 'New Balance', 'Steel Blue', 8, 999.99, 10, '2024-11-11'),
('Asics Gt-2160 Sneakers', 'Asics', 'Silver', 9, 2499.99, 10, '2024-11-11'),
('Reebok Club C Extra Sneaker', 'Reebok', 'Brown-Orange', 3, 3999.99, 10, '2024-11-11'),
('Asics EX89 Sneaker', 'Asics', 'White', 4, 1499.99, 10, '2024-11-11');

SELECT * FROM User_s;

-- Insert sample orders
INSERT INTO Orde_r (UserID, OrderDate, Status, TotalAmount, TrackingNumber)
VALUES
(7, '2024-08-01', 'Pending', 1800.00, 'SA55477779'),
(3, '2024-08-05', 'Shipped', 6000.00, 'SA67900009'),
(6, '2024-08-21', 'Delivered', 3500.00, 'SA67906609'),
(5, '2024-08-14', 'Processing', 4899.00, 'SA67778009'),
(7, '2024-08-24', 'Shipped', 6600.00, 'SA12346867');

SELECT * FROM Orde_r;

UPDATE Orde_r
SET ShippingAddress = '123 New Street, Some City, Some Country'
WHERE OrderID = 5;  -- Update this with the actual OrderID
UPDATE Orde_r
SET ShippingAddress = '789 Street, Another City, Another Country'
WHERE OrderID = 2;

UPDATE Orde_r
SET ShippingAddress = '123 Street, Some City, Some Country'
WHERE OrderID = 3;

UPDATE Orde_r
SET ShippingAddress = '321 Avenue, Yet Another City, Another Country'
WHERE OrderID = 4;

UPDATE Orde_r
SET ShippingAddress = '321 Rosewood, Rd'
WHERE OrderID = 6;
-- Insert sample order items
INSERT INTO OrderItem (OrderID, ProductID, Quantity, Price)
VALUES 
(6, 1, 2, 3999.99),
(5, 2, 1, 999.99);

INSERT INTO Cart (UserID, CreatedDate)
VALUES 
(3, '2024-11-11'), 
(5, '2024-11-11'),  
(6, '2024-11-11');  

INSERT INTO CartItem (CartID, ProductID, Quantity, Price)
VALUES
(1, 5, 1, 1800.00),  
(1, 1, 2, 6000.00),  
(2, 3, 1, 3500.00),  
(2, 4, 1, 4899.00), 
(3, 2, 3, 6600.00);  



-- Insert sample return requests
INSERT INTO ReturnRequest (OrderID, ProductID, Reason, Status,RequestDate)
VALUES 
(2, 1, 'Damaged product', 'Pending', '2024-11-30'),
(3, 2, 'Not as described', 'Approved','2024-11-21');

select * from Notifications;

-- Insert sample messages
INSERT INTO Messages (UserID, MessageText, SentDate)
VALUES 
(3, 'I have an issue with my order.', '2024-11-19'),
(5, 'Could you help me track my package?', '2024-11-12');

-- Insert sample notifications
INSERT INTO Notifications (UserID, NotificationText, CreatedDate)
VALUES 
(3, 'Your order has been shipped!', '2024-11-15'),
(5, 'Your return request has been approved.', '2024-11-15');



