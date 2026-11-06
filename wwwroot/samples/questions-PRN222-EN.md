What is a Port?
A. A 4-byte address that identifies a computer
B. An unsigned 2-byte integer that lets the OS distinguish network-communicating processes
C. The domain name (hostname) of the server
D. A data encapsulation protocol
Đáp án: B
Giải thích: A port is an unsigned 2-byte integer that helps the OS distinguish network-communicating processes.

What are the lengths of the IPv4 and IPv6 addresses, respectively?
A. 2 bytes and 8 bytes
B. 8 bytes and 16 bytes
C. 4 bytes and 16 bytes
D. 4 bytes and 32 bytes
Đáp án: C
Giải thích: IPv4 = 4 bytes (e.g. 192.143.5.1), IPv6 = 16 bytes.

What is a characteristic of TCP (Transmission Control Protocol)?
A. Connectionless, with no acknowledgment mechanism
B. Sends independent datagrams with no delivery guarantee
C. No connection needs to be established before sending
D. Connection-based; guarantees a reliable data stream thanks to the acknowledgment mechanism
Đáp án: D
Giải thích: TCP is connection-based and reliable thanks to acknowledgments.

Which statement about UDP (User Datagram Protocol) is correct?
A. Sends independent datagrams with no guarantee of delivery or correct ordering
B. Always guarantees packet ordering
C. Uses an acknowledgment mechanism
D. Uses only a single connection
Đáp án: A
Giải thích: UDP sends independent datagrams with no delivery/ordering guarantee; the app must handle lost, duplicated and out-of-order packets itself.

What is a URI (Uniform Resource Identifier)?
A. Just the IP address of the server
B. The physical path of a file on disk
C. A specially formatted string describing a resource on the internet or LAN (URL and URN are subsets of URI)
D. A file transfer protocol
Đáp án: C
Giải thích: A URI is a string describing a resource; URL and URN are subsets of it.

What is a characteristic of URN (e.g. `urn:isbn:0451450523`)?
A. It tells the network location where the resource resides
B. It is a URI using the "urn" scheme; it identifies independently of location and does not imply the resource is available for access
C. It always starts with `http`
D. It guarantees the resource is always accessible
Đáp án: B
Giải thích: URN uses the "urn" scheme, is location-independent and does not imply the resource is available. URL is the one that indicates location.

Which class is the abstract base class of the request/response model (HTTP, FTP, `file:`) in a protocol-agnostic way?
A. WebRequest
B. HttpClient
C. TcpClient
D. Dns
Đáp án: A
Giải thích: WebRequest is the abstract base class for HTTP, FTP and `file:`.

How does a client obtain a `WebResponse` object?
A. `new WebResponse()`
B. `WebRequest.Create(Uri)`
C. `WebRequest.Abort()`
D. By calling `GetResponse()` on a `WebRequest`
Đáp án: D
Giải thích: The client does not create a WebResponse itself; it gets one via `GetResponse()`.

How does HttpClient differ from WebRequest/WebClient?
A. It is only used for the FTP protocol
B. It is designed for web APIs/REST, allows plugging in custom message handlers (mockable during unit tests), and each instance has its own connection pool
C. It does not support asynchronous programming
D. It is required to be based on WebRequest in every version of .NET
Đáp án: B
Giải thích: HttpClient fits web APIs/REST, has custom message handlers (mockable during unit tests), and each instance has its own connection pool.

Which method sends a GET and asynchronously returns the response body as a `string`?
A. `PostAsync`
B. `DeleteAsync`
C. `GetStringAsync`
D. `PutAsync`
Đáp án: C
Giải thích: `GetStringAsync` sends a GET and returns the body as a string.

What task does the Domain Name System (DNS) perform?
A. Translates human-readable internet addresses (domain names) into the corresponding IP addresses
B. Encrypts the transmitted data
C. Allocates port numbers to processes
D. Sends email via SMTP
Đáp án: A
Giải thích: DNS maps domain names to IP addresses.

What are the three components of DNS?
A. IP, Port, Protocol
B. Client, Server, Router
C. Socket, Stream, Listener
D. Name Space, Globally Distributed Database (Name Servers), Resolver
Đáp án: D
Giải thích: Name Space, Globally Distributed Database (Name Servers), Resolver.

Which class does the server side use to listen for and accept incoming TCP connections?
A. TcpClient
B. TcpListener
C. UdpClient
D. NetworkStream
Đáp án: B
Giải thích: TcpListener listens for and accepts connections; TcpClient is the side that connects.

Which method of `TcpClient` returns a `NetworkStream` for sending/receiving data?
A. `Accept()`
B. `Listen()`
C. `GetStream()`
D. `Abort()`
Đáp án: C
Giải thích: `GetStream()` returns the `NetworkStream` used to send/receive data.

Which method of the `Socket` class puts a socket into the listening state?
A. `Listen()`
B. `Connect(IPAddress, Int32)`
C. `Send(Byte[])`
D. `Close()`
Đáp án: A
Giải thích: `Listen()` puts the Socket into the listening state; `Accept()` creates a new Socket for an incoming connection.

A mono-processor von Neumann system (input-processing-output) is also known as which model in Flynn's Taxonomy?
A. MIMD
B. SIMD
C. MISD
D. SISD
Đáp án: D
Giải thích: SISD (single instruction, single data); it hits the von Neumann bottleneck.

What does symmetrical multiprocessing (SMP) mean?
A. A master processor acts as a manager distributing tasks to the other processors
B. Any free processor/core can execute a task
C. Only one core can be used
D. Only software optimized for multiprocessing can run
Đáp án: B
Giải thích: SMP: any free processor/core can run the task. Option A describes AMP (asymmetrical multiprocessing).

What is the maximum number of tasks a quad-core processor with Hyper-Threading (HT) can run at the same time?
A. 4
B. 2
C. 8
D. 16
Đáp án: C
Giải thích: Quad-core x HT (2 logical cores per core) = 8 tasks at once.

Which company developed Hyper-Threading (launched on Xeon in 2002)?
A. Intel
B. AMD
C. ARM
D. IBM
Đáp án: A
Giải thích: Intel developed HT, launching it on Xeon in 2002.

Which architecture enables true parallelism and is used by most systems today?
A. SISD
B. MIMD
C. SIMD
D. MISD
Đáp án: B
Giải thích: MIMD provides true parallelism and is used by most modern systems.

Which architecture is commonly used for fault tolerance, e.g. the computer controlling a space shuttle?
A. SISD
B. SIMD
C. MISD
D. MIMD
Đáp án: C
Giải thích: MISD is used for fault tolerance, e.g. the computer controlling a space shuttle.

What is the characteristic of Serial Computing?
A. Instructions run one after another on a single processor; only one instruction is executed at a time
B. The problem is split into parts that run concurrently
C. Multiple processors are used at once
D. A central coordination mechanism is required
Đáp án: A
Giải thích: Serial: instructions run sequentially on a single processor.

What is the limitation of Parallel Computing?
A. It always costs more than serial
B. It cannot be used for real-time simulation
C. It can only run on one CPU
D. Communication and synchronization between sub-tasks is difficult to implement
Đáp án: D
Giải thích: Communication and synchronization between sub-tasks is hard.

Which namespaces contain the TPL (Task Parallel Library)?
A. `System.Net`
B. `System.Threading` and `System.Threading.Tasks`
C. `System.Linq`
D. `System.IO`
Đáp án: B
Giải thích: TPL lives in `System.Threading` and `System.Threading.Tasks`.

Which class do you use when you want a task to return a value?
A. `Task`
B. `TaskFactory`
C. `Task<TResult>`
D. `TaskScheduler`
Đáp án: C
Giải thích: `Task` returns no result; you need `Task<TResult>`.

What does `Task.WhenAll(Task[])` do?
A. Creates a task that completes when all the tasks in the array have completed
B. Completes as soon as the first task finishes
C. Cancels all of the tasks
D. Runs the tasks one by one sequentially
Đáp án: A
Giải thích: `WhenAll` completes when all the child tasks are done.

Which method do you use to turn a LINQ query into PLINQ?
A. `AsSequential()`
B. `AsOrdered()`
C. `ForAll()`
D. `AsParallel()`
Đáp án: D
Giải thích: `AsParallel()` is the entry point of PLINQ.

What does the PLINQ `AsOrdered` operator do?
A. Ignores the order of the source data
B. Asks PLINQ to preserve the order of the source sequence
C. Limits the maximum number of processors
D. Forces the query to run sequentially
Đáp án: B
Giải thích: `AsOrdered` preserves the source sequence order; `AsUnordered` does not need to.

Which is a correct note when using PLINQ?
A. PLINQ is always faster than LINQ
B. You should write to a file/database directly inside a PLINQ query
C. Parallelism has overhead so performance must be measured; `AsParallel()` does not guarantee that the query definitely runs in parallel
D. PLINQ always runs in parallel when `AsParallel()` is present
Đáp án: C
Giải thích: Parallelism has overhead and must be measured; avoid non-thread-safe I/O; `AsParallel` does not guarantee parallel execution.

What do `async` and `await` give you?
A. Writing an async method is almost as easy as a synchronous one, and the calling thread is not blocked waiting
B. They force you to create a new thread for every statement
C. They only run on Windows
D. They completely replace the `Task` class
Đáp án: A
Giải thích: `async/await` lets you write asynchronous code that looks nearly synchronous; the calling thread is not blocked.

Who developed the SOLID principles?
A. Bill Gates
B. Anders Hejlsberg
C. Robert C. Martin (essay in 2000); the SOLID acronym was coined by Michael Feathers
D. Martin Fowler
Đáp án: C
Giải thích: Robert C. Martin (2000); Michael Feathers coined the SOLID acronym.

What does the S (Single Responsibility Principle) say?
A. A class should have only one responsibility (one reason to change)
B. Open for extension, closed for modification
C. A subtype can replace its base type
D. Many specialized interfaces are better than one general interface
Đáp án: A
Giải thích: S: one class, one responsibility.

What does the O (Open/Closed Principle) say?
A. A class has only one responsibility
B. A subtype can replace its base type
C. High-level modules depend on abstractions
D. A software entity is open for extension and closed for modification
Đáp án: D
Giải thích: O: open for extension, closed for modification.

What does the L (Liskov Substitution Principle) say?
A. Many small interfaces are better than one large interface
B. An object can be replaced by an instance of a subtype without making the program incorrect
C. A class has only one responsibility
D. Details must depend on abstractions
Đáp án: B
Giải thích: L: a subtype can substitute without breaking the program.

What does the I (Interface Segregation Principle) say?
A. Use one very large general interface for every client
B. Interfaces should not be used
C. Many specialized interfaces for each client are better than one general-purpose interface
D. High-level modules depend on low-level modules
Đáp án: C
Giải thích: I: many specialized interfaces beat one general interface.

What does the D (Dependency Inversion Principle) say?
A. High-level modules should not depend on low-level modules; both should depend on abstractions
B. Low-level modules must depend on high-level modules
C. Abstractions depend on details
D. Do not use interfaces in the design
Đáp án: A
Giải thích: D: both high-level and low-level depend on abstractions.

Class `Notification` directly creates `new Email()` in its constructor. What is this?
A. Loosely coupled
B. Following the Dependency Inversion Principle
C. Following the Open/Closed Principle
D. Tightly coupled: to send another kind of notification you have to modify `Notification`
Đáp án: D
Giải thích: `new Email()` in the constructor is tightly coupled, violating DIP and OCP.

What is the relationship between DIP and IoC (Inversion of Control)?
A. The two concepts are identical
B. DIP is a principle (theory); IoC is a pattern with concrete steps to make code DIP-friendly
C. IoC is a Microsoft framework
D. DIP is a pattern, IoC is a principle
Đáp án: B
Giải thích: DIP is a principle/theory; IoC is a pattern with concrete steps to make code DIP-friendly.

What is Dependency Injection (DI)?
A. Creating an object with `new` inside the class that uses it
B. Using a static class for every dependency
C. A way of implementing IoC by injecting a concrete implementation into the class that is using an abstraction (interface)
D. A special kind of interface
Đáp án: C
Giải thích: DI is a way to implement IoC: inject an implementation into the class that uses an interface.

What are the three basic capabilities of a DI Container?
A. Registration, Resolution, Disposition
B. Compile, Link, Run
C. Create, Read, Update
D. Connect, Send, Receive
Đáp án: A
Giải thích: Registration, Resolution (create and inject), Disposition (lifetime management).

Which service lifetime creates a new instance every time it is requested?
A. Singleton
B. Scoped
C. Static
D. Transient
Đáp án: D
Giải thích: Transient creates a new instance per request; Singleton uses a single instance; Scoped is limited to one context.

How does Ambient Context provide a dependency?
A. Passing it through the constructor
B. Through a static accessor, avoiding cluttering the API with cross-cutting concerns
C. Passing it as a method parameter
D. Through a writable property
Đáp án: B
Giải thích: Ambient Context provides the dependency through a static accessor.

Which DI pattern ensures the dependency is always available because every caller is required to provide it through the constructor?
A. Property Injection
B. Method Injection
C. Constructor Injection
D. Ambient Context
Đáp án: C
Giải thích: Constructor Injection: the most common one; it ensures the dependency exists when the object is created.

What is the drawback of Property Injection?
A. It hides the dependency and the property can be null, so null checks are needed everywhere it is used
B. It ensures the dependency is always present at construction
C. It forces you to change the constructor
D. It can only be used with static classes
Đáp án: A
Giải thích: Property Injection hides the dependency and it can be null, easily causing a `NullReferenceException`.

When the dependency changes on every call (a different dependency per operation), which pattern should you use?
A. A dependency fixed for the whole lifetime of the object
B. A global variable
C. An optional property
D. Method Injection (pass the dependency as a method parameter)
Đáp án: D
Giải thích: Method Injection: the dependency differs per call and is passed as a parameter.

What is a characteristic of ASP.NET Core?
A. It only runs on Windows and requires `System.Web.dll`
B. Free, open-source, cross-platform, lightweight modular architecture and no dependency on `System.Web.dll`
C. It completely replaces C#
D. It can only run on IIS
Đáp án: B
Giải thích: Free, open-source, cross-platform, modular, and no longer dependent on `System.Web.dll`.

What are the limitations of ASP.NET Web Forms that led to needing a new framework?
A. Easy to test and complete control over HTML
B. A simple stateless model
C. It only runs on Linux
D. Lack of testability, a complex stateful model and little control over the generated HTML
Đáp án: D
Giải thích: Web Forms: lack of testability, complex stateful model, little control over HTML.

What are the two main tasks of Routing?
A. Map an incoming request to a Controller Action; generate the corresponding outgoing URLs for the action
B. Render the view and save to the database
C. Encrypt the URL and compress the response
D. Manage sessions and cookies
Đáp án: A
Giải thích: Routing maps request to action and generates outgoing URLs.

In the pattern `{controller=Home}/{action=Index}/{id?}`, what does the `?` in `id?` mean?
A. The id parameter is required
B. Id is always null
C. The id parameter is optional
D. Id is the controller name
Đáp án: C
Giải thích: `?` means the parameter is optional.

What is the role of the Controller in MVC?
A. It only renders the HTML interface
B. Handles user interactions/requests, runs the necessary logic and returns a response (orchestrating the logic flow)
C. It only stores data into the database
D. It only contains CSS and JavaScript
Đáp án: B
Giải thích: The Controller orchestrates the logic flow and returns the response.

What is the responsibility of the View in MVC?
A. Holding business logic
B. Accessing the database
C. Handling incoming requests
D. Only rendering the model to the user; it should not contain processing logic
Đáp án: D
Giải thích: The View only renders; it does not process logic.

What is a ViewModel?
A. A class containing only the properties the view needs; it is optional and helps expose only the necessary data
B. A class that must be present in every view
C. It is the DbContext
D. It is the Controller
Đáp án: A
Giải thích: A ViewModel holds the data specific to the view; it is optional.

What is the condition for an Action method?
A. It can be private
B. It can be protected
C. It must be public and must not be static
D. It can be static
Đáp án: C
Giải thích: An action method must be `public` and must not be `static`.

If a view is not found in the `Views/<ControllerName>` folder, where does the framework look next?
A. It throws an error immediately
B. The `Views/Shared` folder
C. The `wwwroot` folder
D. The `Models` folder
Đáp án: B
Giải thích: A view not found in `Views/<Controller>` is searched for next in `Views/Shared`.

What is Model Binding?
A. Encrypting the form data
B. Checking access permissions
C. Creating a table in the database
D. Mapping data from the HTTP request (form, route, query string, body) into the parameters of an action method
Đáp án: D
Giải thích: Model binding maps request data into the action parameters.

When does Model Validation take place?
A. After model binding and before the action executes; errors are stored in `ModelState`
B. After the action has already finished running
C. Before model binding
D. It only takes place on the client
Đáp án: A
Giải thích: Validation runs after binding and before the action; the result lives in `ModelState`.

What is the `[Compare]` attribute used for?
A. Checking the email format
B. Checking a value within a range
C. Checking that two properties in the model match (e.g. Password and ConfirmPassword)
D. Checking the URL format
Đáp án: C
Giải thích: `[Compare]` matches two properties against each other.

Compare ViewData and ViewBag?
A. ViewBag is a key/value dictionary, ViewData is dynamic
B. ViewData is a key/value dictionary; ViewBag is similar but accessed through dynamic properties
C. The two are completely unrelated
D. Only ViewBag can pass data from the controller to the view
Đáp án: B
Giải thích: ViewData is a dictionary; ViewBag is a wrapper accessed using dynamic properties.

What is TempData used for?
A. It only exists within a single view
B. Storing permanently into the database
C. It only exists on the client side
D. Temporary storage, usually for one-time messages/passing data between actions; `TempData.Keep()` keeps the data after the view is rendered
Đáp án: D
Giải thích: TempData holds temporary/one-time data; `Keep()` retains it after rendering.

What is the storage scope of `HttpContext.Items`?
A. Only within a single request; it is discarded after the request has been processed
B. Across many requests of the same user
C. Stored in the browser's cookie
D. Stored in the Session
Đáp án: A
Giải thích: `HttpContext.Items` lives only for a single request.

What is Razor Pages?
A. A framework that runs entirely client-side
B. A framework that can only be used on Windows
C. A server-side, page-focused framework for building dynamic websites, based on MVC and available since .NET Core 2.0
D. A library that replaces the C# language
Đáp án: C
Giải thích: Server-side, page-focused, available since .NET Core 2.0.

Which two files make up a Razor Page together with its PageModel?
A. `Demo2.cshtml` and `Demo2.cshtml.cs` (same name, mapped 1-1)
B. `Demo2.razor` and `Demo2.cs`
C. `Demo2.html` and `Demo2.js`
D. `Demo2.cshtml` and `Demo2.json`
Đáp án: A
Giải thích: `.cshtml` + `.cshtml.cs`, mapped 1-1.

What is the naming convention for handler methods in Razor Pages?
A. `Get()`, `Post()`
B. `HandleGet()`, `HandlePost()`
C. `get()`, `post()`
D. The "On" prefix: `OnGet()`, `OnPost()`; `OnGet` and `OnGetAsync` are considered the same handler
Đáp án: D
Giải thích: The "On" + verb convention: `OnGet`, `OnPost`; the async version is the same handler.

What is `_Layout.cshtml` used for?
A. Declaring global directives
B. Serving as the template for the content pages that reference it (the consistent UI of the site)
C. Running code after every content page
D. Rendering partial pages
Đáp án: B
Giải thích: `_Layout` is the shared template for the pages.

What is the role of `_ViewStart.cshtml`?
A. Declaring global namespaces
B. Participating in routing like a normal page
C. Holding code that runs after the code of the content page in the same folder/subfolder; a convenient place to specify the layout
D. Storing the connection string
Đáp án: C
Giải thích: `_ViewStart` runs after the content page code, making it convenient for specifying the layout.

What is `_ViewImports.cshtml` used for?
A. Gathering the shared directives (`@using`, `@addTagHelper`, `@inject`, `@model`...) for the Razor pages
B. Serving as the layout template
C. Managing the Session
D. Connecting to the database
Đáp án: A
Giải thích: `_ViewImports` gathers the shared directives.

What is a characteristic of Partial Pages?
A. They participate in routing
B. They must have a PageModel
C. They completely replace the layout
D. They do not participate in routing; rendered with `<partial name="_MenuPartial" />`
Đáp án: D
Giải thích: A partial page does not participate in routing; it is rendered with `<partial />`.

What is the PageModel in Razor Pages?
A. An implementation of the MVVM pattern
B. An implementation of the Page Controller pattern (one page to one controller), acting as both controller and view model
C. An implementation of a Front Controller handling every request of the site
D. Just a pure-data ViewModel
Đáp án: B
Giải thích: PageModel = the Page Controller pattern; not MVVM, not a Front Controller.

When should you use a View Component instead of a partial page or a tag helper?
A. When you only need static HTML
B. When you do not need unit tests
C. When you need logic to fetch data (file, database, web service); the class inherits `ViewComponent` and has an `Invoke` method
D. When you want to replace routing
Đáp án: C
Giải thích: A View Component is used when you need logic to fetch data; the class inherits `ViewComponent` and has `Invoke`.

In Razor Pages routing, at which URL(s) can the `Index.cshtml` file in the root `Pages` folder be accessed?
A. Both `/` and `/Index`
B. Only `/Index`
C. Only `/Home`
D. It cannot be accessed
Đáp án: A
Giải thích: `Index.cshtml` is accessible via both `/` and `/Index`.

Why is server-side validation still needed when client-side validation already exists?
A. Client-side validation does not exist
B. Server-side is faster so it replaces the client
C. Server-side does not need DataAnnotations
D. Client-side is very easy to bypass; the server runs the validation attributes and `ModelState.IsValid` returns `false` if there are errors
Đáp án: D
Giải thích: Client-side validation is easy to bypass, so the server must check again. Tester tip: try disabling JavaScript and then submitting invalid data.

What does the `[BindProperty]` attribute on a public property of a PageModel do?
A. Attaches the property to a handler method
B. Binds the posted form value into the property (convenient for using values outside the handler and for binding with tag helpers)
C. Automatically validates all data
D. Defines the route for the page
Đáp án: B
Giải thích: `[BindProperty]` binds the form value into the PageModel property.

What are Tag Helpers in Razor Pages?
A. A JavaScript library for manipulating the DOM
B. A tool for generating database code
C. Reusable components that automatically generate HTML for specific tags; enabled with `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`
D. A type of layout file
Đáp án: C
Giải thích: A Tag Helper automatically generates HTML for specific tags and is enabled with `@addTagHelper`.

Which statement about Session state in Razor Pages is correct?
A. It is not enabled by default; it must be enabled in the Startup/app configuration file, and a cookie is used to tie the requests together into one session
B. It is enabled by default
C. No cookie is needed
D. It is only stored on the client side
Đáp án: A
Giải thích: Session is not enabled by default and uses a cookie to link the requests together.

What happens to a request that does not go through HTTPS when the `RequireHttps` attribute is used?
A. It is accepted normally
B. It always returns 404 Not Found
C. It is always redirected to HTTPS regardless of which verb is used
D. A GET request is redirected to the HTTPS version; other verbs (e.g. POST) receive 403 Forbidden
Đáp án: D
Giải thích: GET is redirected to HTTPS; other verbs such as POST get 403 Forbidden.

Which two words is the name "Blazor" built from?
A. Block + Razor
B. Browser + Razor
C. Binary + Lazor
D. Backend + Azure
Đáp án: B
Giải thích: Blazor = Browser + Razor.

In Blazor Server, through what are the UI interactions handled?
A. SignalR connection
B. FTP
C. SMTP
D. Remote Desktop
Đáp án: A
Giải thích: Blazor Server handles UI through a SignalR connection.

Which kind of application is Blazor WebAssembly best suited for?
A. An internal application that needs a constant connection to the server
B. An application that only runs on the server
C. An offline-capable or public application (the first download is large, but afterwards no server interaction is needed)
D. An application that does not use C#
Đáp án: C
Giải thích: WASM suits offline-capable or public apps; the first-time download is heavy.

In which file is a Blazor component usually defined, and how is it used?
A. A `.cshtml` file, used with `@Component("MyComponent")`
B. A `.cs` file, used with `new MyComponent()`
C. An `.html` file, used with `<include MyComponent>`
D. A `.razor` file, used like an element: `<MyComponent />`
Đáp án: D
Giải thích: The component lives in a `.razor` file and is used like `<MyComponent />`.

Which directive declares the route `/todo` for a component?
A. `@route "/todo"`
B. `@page "/todo"`
C. `@url "/todo"`
D. `@path "/todo"`
Đáp án: B
Giải thích: `@page "/todo"` declares the route.

What kind of binding does `<input @bind="newTodo" />` perform?
A. Two-way binding between the input box and the `newTodo` variable
B. Only one-way (displaying the value)
C. No binding at all
D. Only importing a namespace
Đáp án: A
Giải thích: `@bind` is two-way binding.

Which attribute attaches a C# method to the click event of a button?
A. `@click`
B. `onpress`
C. `@onclick`
D. `@submit`
Đáp án: C
Giải thích: `@onclick` attaches a C# method to the click event.

What is the role of the `Routes.razor` file in a Blazor Web App project?
A. It holds the connection string
B. It declares the shared namespaces
C. It is the entry point that starts the server
D. It configures the Blazor router (`App.razor` is the root component)
Đáp án: D
Giải thích: `Routes.razor` configures the router; `App.razor` is the root component.

What is the `_Imports.razor` file used for?
A. Configuring the router
B. Declaring the namespaces commonly shared by the components
C. Holding the static files
D. Being the entry point of the application
Đáp án: B
Giải thích: `_Imports.razor` declares the shared namespaces.

What are Cascading Values (cascading parameters) used for in Blazor?
A. Sharing data between nested components
B. Removing data from a component
C. Navigating to another page
D. Compiling C# code
Đáp án: A
Giải thích: Cascading parameters share data between nested components.

What does the line `@rendermode InteractiveServer` at the top of a component do?
A. Turns interactivity completely off
B. Forces the component to run on WebAssembly
C. Turns on interactivity for the component in Interactive Server mode
D. Only renders static HTML
Đáp án: C
Giải thích: `@rendermode InteractiveServer` turns on Server-style interactivity.

Which command applies the migrations to the database in the Movie demo (EF Core)?
A. `dotnet ef migrations add "InitialDB"` (this command only creates the migration file)
B. `dotnet new blazor`
C. `dotnet run`
D. `dotnet ef database update`
Đáp án: D
Giải thích: `database update` applies the migrations; `migrations add` only creates the migration file.

What does the `[Column(TypeName = "decimal(18, 2)")]` attribute on the `Price` property do?
A. Formats the display on the UI
B. Specifies the column type in the database
C. Forces the user to enter a price
D. Renames the table
Đáp án: B
Giải thích: `[Column(TypeName=...)]` specifies the column type in the DB.

How does "Pushing" differ from "Pulling"?
A. Push means being notified of what is new, instead of having to go ask what is new (Pull)
B. Push means the client constantly asks the server
C. Pull means the server sends data down to the client by itself
D. The two concepts are the same
Đáp án: A
Giải thích: Push = being told what is new; Pull = going to ask.

What is the transport fallback order in SignalR?
A. Long Polling → Server-Sent Events → WebSockets
B. Server-Sent Events → WebSockets → Long Polling
C. WebSockets → Server-Sent Events → Long Polling
D. Long Polling → WebSockets → Server-Sent Events
Đáp án: C
Giải thích: WebSockets to Server-Sent Events to Long Polling.

How do you create a Hub in SignalR?
A. Create a class implementing `IDisposable`
B. Create a static class
C. Create an `IHub` interface
D. Declare a class that inherits from `Hub` and add public methods
Đáp án: D
Giải thích: A Hub is a class inheriting from `Hub`; its public methods are what clients can call.

What do you need in order to configure SignalR in `Program.cs`?
A. Only `MapHub`
B. `AddSignalR()` registers the services; `MapHub<ChatHub>("/chatHub")` maps the hub to a URL endpoint
C. Only installing the JavaScript library on the client
D. No configuration is needed
Đáp án: B
Giải thích: `AddSignalR()` + `MapHub<T>(url)`.

On which objects does `Clients.Others` invoke the method?
A. All connected clients except the client that called the hub method
B. Only the client that called the hub method
C. Every connected client
D. Only one group
Đáp án: A
Giải thích: `Others` = every client except the caller.

What does `Context.ConnectionId` return?
A. The user name
B. The IP address
C. A unique ID assigned by SignalR to each connection
D. The group name
Đáp án: C
Giải thích: `ConnectionId` is the unique ID of each connection.

How do you fix the "magic string" drawback of `SendAsync("ReceiveMessage", ...)`?
A. Use JSON instead of MessagePack
B. Use a hub filter
C. Use `Context.Items`
D. A strongly typed hub with `Hub<T>` to avoid runtime errors caused by a wrong method name
Đáp án: D
Giải thích: `Hub<T>` catches a wrong name at compile time instead of causing a runtime error.

What does the `[HubMethodName("SendMessageToUser")]` attribute do?
A. Renames the hub class
B. Specifies the name the client uses to call the method, instead of the default .NET method name
C. Hides the method from the client
D. Configures a group
Đáp án: B
Giải thích: `[HubMethodName]` renames the method that the client calls.

In `OnDisconnectedAsync(Exception exception)`, when is the `exception` parameter `null`?
A. When the client disconnects intentionally (e.g. calls `connection.stop()`)
B. When there is a network failure
C. When the server crashes
D. When the connection times out
Đáp án: A
Giải thích: `null` on an intentional disconnect; there is an exception when the disconnect is caused by an error such as a network fault.

Which statement about Hub filters is correct?
A. They only existed before ASP.NET Core 3.1
B. They only run on the client side
C. They exist since ASP.NET Core 5.0, implement `IHubFilter` with `InvokeMethodAsync`; global filters run before local filters
D. They completely replace the `Hub` class
Đáp án: C
Giải thích: Hub filters exist since ASP.NET Core 5.0, implement `IHubFilter`; global runs before local.

How does MessagePack compare to JSON in SignalR?
A. It is larger and slower than JSON
B. It is easier to read with the naked eye
C. They are exactly the same
D. It is a compact binary serialization and is faster (e.g. in the slide: JSON 27 bytes, MessagePack 18 bytes)
Đáp án: D
Giải thích: MessagePack is smaller and faster than JSON (18 bytes versus 27 bytes in the example).

In the Entity Framework + SignalR lab, why do we call `Clients.All.SendAsync("LoadProducts")` after `SaveChangesAsync()`?
A. To save the data into the database
B. To tell every client to reload the product list (the client calls `LoadProdData()` and redraws the table), keeping the data synchronized in real time
C. To create a new migration
D. To clear the server cache
Đáp án: B
Giải thích: It tells every client to reload the list so the data stays synchronized in real time.
