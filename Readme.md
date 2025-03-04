software for managing customers and printing invoices.

- creating and managing customer and invoice-item data
- booking paid amount per customer
- creating invoices for each customer
- creating other relevant documents by predefining templates

1. frameworks used in this project

- EntityFramework with SqLite database
- PdfSharp
- ASP.Net web application

2. solutions to different problems

2.1 internal communication between elements

- using a standardized [ReturnDialog(T)] as communication object
    - /Controller/Data/ReturnDialog.cs
- standardized or dynamic messages
- boolean whether process completed successfully
- generic (T) as return value, for example [ReturnDialog(UserAccount)]

2.2 Authentification

- using Rfc2898 Algorithm for hashing
- randomly creating hash parameter (salt, length, iterations): 
    /Controller/Security/RandomHashParamater.cs
- creating hashed password from username and plaintextpassword
- saving username, password and hashparameter in database as [UserAccount]
- comparing submitted values of username and password to saved objects
    /Controller/Security/Security.cs

2.3 Database

- using ef with local sqlite database (/Database)
- using global variable [Year] to reuse program each year
- using migrations for creation and seeding initial data (/Migrations)
    /DatabaseModel/CustomerManagementContext.cs
- objects of database
    - [UserAccount] for authenification
    - [Customer] data of customer
    - [InvoiceItem] item of an invoice
    - [CustomerInvoiceItem] cross table of [Customer] and [InvoiceItem]
    - [CustomerFile] saved files of a customer
    - [CustomerBooking] history of bookings of customer
    - [Log] list of error logs occuring in program
    /DatabaseModel/CustomerManagementsObjects.cs
- functions of database are realized using transactions where needed, creating the database objects and disposing of it when the function ends
    /Controller/Data/UserManagementPacker.cs

2.4 Session

- storing an authenticated user in a global session
    /Global/Session/Session.cs
- accessing functions of database with functions of authenticated user stored in session
    /Global/Session/AuthenticatedUser.cs
- roles of authenticated users are <User>,<Admin> but are not relevant for now

2.5 DataProcessing

- there is a static helper class to calculate sums and missing amounts 
    /Controller/Data/CalculateData.cs
- sometimes the stored value is not the final currency value but for example a value of squarefoot
- to transform the stored value into the final value a [TransformFormula] stored with the [InvoiceItem] is needed
- the formula contains numbers, signs and the character 'W' as placeholder for the stored value
- for example to transform a value of <200 squarefoot> into the the value of <1000 $> the formula would be '5*W'
    /Controller/Data/TransformFormula.cs

2.6 DataModels and InputValidation

- for each page where input is submitted a datamodel for the needed data is created
- each model has a validate function which uses static functions to validate submitted data returning a [ReturnDialog]
- the returned [ReturnDialog] contains a message, which data was invalid if so
    /Pages/DataModelValidation.cs

2.7 Pages, CSS and ColorModes

- a global static variable [ColorMode] is used to switch between different color modes (<Red>, <Blue> for now)
- each css class has a suffix <-color> to differentiate between the color modes
- there are global css variables for colors, each with a prefix <Color-> which are used by css classes
- each page.cshtml has a switch case referencing the static global variable to print html code referencing the different css classes by color
- all pages reference a global site.css
    /wwwroot/css/site.css

2.8 Partials

- the page <Management> has different partial with a static variable realized as an enum of the partials
- the index page switches between the partial pages with the held of post-functions according to the stored value in the static variable
- to ensure saving of data between navigating of pages the nullable ids of the picked [Customer] and [InvoiceItem] are stored in a static variable
- the partials then load data according to the stored values
    /Pages/Management/...

2.9 Document Templates and Printing

- a document template contains static and dynamic elements
- static elements have a fixed position on the page, and can be repeated for each new page
- the next dynamic element has a vertical position with a distance from the last dynamic element
- inside an element, subelements can be static or dynamic. here dynamic elements have a horizontal position with distance from the last dynamic element
- subelements can be strings, geometries or images
- strings use [FormatStrings] to dynamically insert data into the template 
- a template can decide which data it needs, for example an [UserAccount], an [InvoiceItem] or lists of those
- created documents can then be stored in the database as [CustomerFile]
    /Controller/PdfWriter/...