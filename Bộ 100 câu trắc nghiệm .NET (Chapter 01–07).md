# Bộ 100 câu trắc nghiệm .NET (Chapter 01–07)

*Nguồn: 7 slide bài giảng FPT University (Networking, Async/Parallel, Dependency Injection, ASP.NET Core MVC, Razor Pages, Blazor, SignalR).*

**Cách dùng:** làm hết 100 câu trước, rồi mới xem **Đáp án & giải thích** ở cuối. Câu nào sai, hãy tự hỏi "vì sao đáp án đúng lại hợp lý?" thay vì chỉ học thuộc chữ cái.

**Phân bố:** Ch01 (câu 1–15) · Ch02 (16–30) · Ch03 (31–45) · Ch04 (46–60) · Ch05 (61–75) · Ch06 (76–88) · Ch07 (89–100)

## Chapter 01: Networking Programming

**1.** Port là gì?

- A. Địa chỉ 4 byte định danh một máy tính
- B. Số nguyên không dấu 2 byte giúp hệ điều hành phân biệt các tiến trình giao tiếp mạng
- C. Tên miền (hostname) của server
- D. Giao thức đóng gói dữ liệu

**2.** Độ dài của địa chỉ IPv4 và IPv6 lần lượt là?

- A. 2 byte và 8 byte
- B. 8 byte và 16 byte
- C. 4 byte và 16 byte
- D. 4 byte và 32 byte

**3.** Đặc điểm của TCP (Transmission Control Protocol) là gì?

- A. Connectionless, không có cơ chế xác nhận
- B. Gửi các datagram độc lập, không đảm bảo đến nơi
- C. Không cần thiết lập kết nối trước khi gửi
- D. Connection-based, đảm bảo luồng dữ liệu tin cậy nhờ cơ chế acknowledge

**4.** Phát biểu nào đúng về UDP (User Datagram Protocol)?

- A. Gửi các datagram độc lập, không đảm bảo đến nơi hay đúng thứ tự
- B. Luôn đảm bảo đúng thứ tự gói tin
- C. Dùng cơ chế acknowledge để xác nhận
- D. Chỉ dùng duy nhất một đường kết nối

**5.** URI (Uniform Resource Identifier) là gì?

- A. Chỉ là địa chỉ IP của server
- B. Đường dẫn vật lý của file trên ổ đĩa
- C. Chuỗi định dạng đặc biệt mô tả một tài nguyên trên internet hoặc LAN (URL và URN là tập con của URI)
- D. Giao thức truyền file

**6.** Đặc điểm của URN (ví dụ `urn:isbn:0451450523`) là gì?

- A. Cho biết vị trí mạng nơi tài nguyên nằm
- B. Là URI dùng scheme "urn", định danh độc lập vị trí và không ngụ ý tài nguyên có sẵn để truy cập
- C. Luôn bắt đầu bằng `http`
- D. Đảm bảo tài nguyên luôn truy cập được

**7.** Lớp nào là abstract base class của mô hình request/response (HTTP, FTP, `file:`) theo kiểu không phụ thuộc giao thức?

- A. WebRequest
- B. HttpClient
- C. TcpClient
- D. Dns

**8.** Client lấy được đối tượng `WebResponse` bằng cách nào?

- A. `new WebResponse()`
- B. `WebRequest.Create(Uri)`
- C. `WebRequest.Abort()`
- D. Gọi `GetResponse()` trên một `WebRequest`

**9.** HttpClient khác WebRequest/WebClient ở điểm nào?

- A. Chỉ dùng cho giao thức FTP
- B. Thiết kế cho web API/REST, cho phép plug custom message handlers (mock khi unit test), mỗi instance có connection pool riêng
- C. Không hỗ trợ lập trình bất đồng bộ
- D. Bắt buộc dựa trên WebRequest trong mọi phiên bản .NET

**10.** Method nào gửi GET và trả về response body dạng `string` theo kiểu bất đồng bộ?

- A. `PostAsync`
- B. `DeleteAsync`
- C. `GetStringAsync`
- D. `PutAsync`

**11.** Domain Name System (DNS) làm nhiệm vụ gì?

- A. Chuyển địa chỉ Internet dạng gợi nhớ (tên miền) thành địa chỉ IP tương ứng
- B. Mã hóa dữ liệu truyền đi
- C. Cấp phát số port cho tiến trình
- D. Gửi email qua SMTP

**12.** Ba thành phần của DNS là gì?

- A. IP, Port, Protocol
- B. Client, Server, Router
- C. Socket, Stream, Listener
- D. Name Space, Globally Distributed Database (Name Servers), Resolver

**13.** Lớp nào phía server dùng để lắng nghe và chấp nhận kết nối TCP đến?

- A. TcpClient
- B. TcpListener
- C. UdpClient
- D. NetworkStream

**14.** Method nào của `TcpClient` trả về `NetworkStream` để gửi/nhận dữ liệu?

- A. `Accept()`
- B. `Listen()`
- C. `GetStream()`
- D. `Abort()`

**15.** Method nào của lớp `Socket` đặt socket vào trạng thái lắng nghe?

- A. `Listen()`
- B. `Connect(IPAddress, Int32)`
- C. `Send(Byte[])`
- D. `Close()`

## Chapter 02: Asynchronous and Parallel Programming

**16.** Hệ mono-processor kiểu von Neumann (input-processing-output) còn được gọi là mô hình nào trong Flynn's Taxonomy?

- A. MIMD
- B. SIMD
- C. MISD
- D. SISD

**17.** Symmetrical multiprocessing (SMP) có nghĩa là gì?

- A. Một processor chính làm manager phân phát task cho các processor khác
- B. Bất kỳ processor/core rảnh nào cũng có thể thực thi task
- C. Chỉ dùng được một core
- D. Chỉ chạy được phần mềm tối ưu cho đa xử lý

**18.** Một processor có chip quad-core bật Hyper-Threading (HT) chạy được tối đa bao nhiêu task cùng lúc?

- A. 4
- B. 2
- C. 8
- D. 16

**19.** Công nghệ Hyper-Threading do hãng nào phát triển (ra mắt trên Xeon năm 2002)?

- A. Intel
- B. AMD
- C. ARM
- D. IBM

**20.** Kiến trúc nào cho phép true parallelism và được hầu hết hệ thống hiện nay sử dụng?

- A. SISD
- B. MIMD
- C. SIMD
- D. MISD

**21.** Kiến trúc nào thường dùng cho fault tolerance (chịu lỗi), ví dụ máy tính điều khiển tàu con thoi?

- A. SISD
- B. SIMD
- C. MISD
- D. MIMD

**22.** Đặc điểm của Serial Computing (tính toán tuần tự) là gì?

- A. Các lệnh chạy lần lượt trên một processor, tại một thời điểm chỉ một lệnh được thực thi
- B. Bài toán chia thành các phần chạy đồng thời
- C. Dùng nhiều processor cùng lúc
- D. Cần cơ chế điều phối trung tâm

**23.** Hạn chế của Parallel Computing là gì?

- A. Luôn tốn nhiều chi phí hơn serial
- B. Không dùng được cho mô phỏng thời gian thực
- C. Chỉ chạy được trên một CPU
- D. Giao tiếp và đồng bộ (synchronization) giữa các sub-tasks khó thực hiện

**24.** TPL (Task Parallel Library) nằm trong các namespace nào?

- A. `System.Net`
- B. `System.Threading` và `System.Threading.Tasks`
- C. `System.Linq`
- D. `System.IO`

**25.** Muốn một task trả về giá trị, ta dùng lớp nào?

- A. `Task`
- B. `TaskFactory`
- C. `Task<TResult>`
- D. `TaskScheduler`

**26.** `Task.WhenAll(Task[])` làm gì?

- A. Tạo task hoàn thành khi tất cả task trong mảng đã hoàn thành
- B. Hoàn thành ngay khi task đầu tiên xong
- C. Hủy tất cả các task
- D. Chạy các task lần lượt tuần tự

**27.** Dùng method nào để chuyển một truy vấn LINQ sang PLINQ?

- A. `AsSequential()`
- B. `AsOrdered()`
- C. `ForAll()`
- D. `AsParallel()`

**28.** Operator `AsOrdered` của PLINQ có tác dụng gì?

- A. Bỏ qua thứ tự của nguồn dữ liệu
- B. Yêu cầu PLINQ giữ nguyên thứ tự của chuỗi nguồn
- C. Giới hạn số processor tối đa
- D. Bắt query chạy tuần tự

**29.** Lưu ý đúng khi dùng PLINQ là gì?

- A. PLINQ luôn nhanh hơn LINQ
- B. Nên ghi file/database trực tiếp bên trong query PLINQ
- C. Parallel có overhead nên cần đo hiệu năng; `AsParallel()` không đảm bảo chắc chắn chạy song song
- D. PLINQ luôn chạy song song khi có `AsParallel()`

**30.** `async` và `await` giúp được gì?

- A. Viết async method gần như dễ như synchronous, thread gọi không bị chặn chờ
- B. Bắt buộc tạo thread mới cho mỗi lệnh
- C. Chỉ chạy được trên Windows
- D. Thay thế hoàn toàn lớp `Task`

## Chapter 03: Dependency Injection in .NET

**31.** Các nguyên lý SOLID do ai phát triển?

- A. Bill Gates
- B. Anders Hejlsberg
- C. Robert C. Martin (bài luận năm 2000); tên viết tắt SOLID do Michael Feathers đặt
- D. Martin Fowler

**32.** Chữ **S** (Single Responsibility Principle) nói gì?

- A. Một class chỉ nên có một trách nhiệm (một lý do để thay đổi)
- B. Mở cho mở rộng, đóng với sửa đổi
- C. Subtype thay thế được type cha
- D. Nhiều interface chuyên biệt tốt hơn một interface chung

**33.** Chữ **O** (Open/Closed Principle) nói gì?

- A. Một class chỉ có một trách nhiệm
- B. Subtype thay thế được type cha
- C. Module cấp cao phụ thuộc vào abstraction
- D. Software entity mở cho mở rộng (extension), đóng với sửa đổi (modification)

**34.** Chữ **L** (Liskov Substitution Principle) nói gì?

- A. Nhiều interface nhỏ tốt hơn một interface lớn
- B. Đối tượng có thể được thay bằng instance của subtype mà không làm sai tính đúng đắn của chương trình
- C. Class chỉ có một trách nhiệm
- D. Details phải phụ thuộc vào abstractions

**35.** Chữ **I** (Interface Segregation Principle) nói gì?

- A. Dùng một interface chung thật lớn cho mọi client
- B. Không nên dùng interface
- C. Nhiều interface chuyên biệt cho từng client tốt hơn một interface đa dụng
- D. High-level module phụ thuộc low-level module

**36.** Chữ **D** (Dependency Inversion Principle) nói gì?

- A. High-level module không nên phụ thuộc low-level module; cả hai nên phụ thuộc vào abstractions
- B. Low-level module phải phụ thuộc vào high-level module
- C. Abstractions phụ thuộc vào details
- D. Không dùng interface trong thiết kế

**37.** Class `Notification` tạo trực tiếp `new Email()` trong constructor. Đây là dạng gì?

- A. Loosely coupled
- B. Tuân thủ Dependency Inversion Principle
- C. Tuân thủ Open/Closed Principle
- D. Tightly coupled: muốn gửi loại thông báo khác thì phải sửa lại `Notification`

**38.** Quan hệ giữa DIP và IoC (Inversion of Control) là gì?

- A. Hai khái niệm giống hệt nhau
- B. DIP là nguyên tắc (lý thuyết); IoC là pattern với các bước cụ thể để code thân thiện với DIP
- C. IoC là một framework của Microsoft
- D. DIP là pattern, IoC là nguyên tắc

**39.** Dependency Injection (DI) là gì?

- A. Tạo object bằng `new` bên trong class sử dụng nó
- B. Dùng static class cho mọi dependency
- C. Cách thực hiện IoC bằng việc inject implementation cụ thể vào class đang dùng abstraction (interface)
- D. Một loại interface đặc biệt

**40.** Ba tính năng cơ bản của DI Container là gì?

- A. Registration, Resolution, Disposition
- B. Compile, Link, Run
- C. Create, Read, Update
- D. Connect, Send, Receive

**41.** Service lifetime nào tạo instance mới mỗi lần được yêu cầu?

- A. Singleton
- B. Scoped
- C. Static
- D. Transient

**42.** Ambient Context cung cấp dependency bằng cách nào?

- A. Truyền qua constructor
- B. Qua một static accessor, tránh làm rối API bởi các cross-cutting concerns
- C. Truyền qua method parameter
- D. Qua writable property

**43.** Pattern DI nào đảm bảo dependency luôn có sẵn vì mọi caller bắt buộc cung cấp nó qua constructor?

- A. Property Injection
- B. Method Injection
- C. Constructor Injection
- D. Ambient Context

**44.** Nhược điểm của Property Injection là gì?

- A. Ẩn dependency và property có thể null nên cần kiểm tra null ở mọi nơi dùng
- B. Đảm bảo dependency luôn có khi khởi tạo
- C. Bắt buộc phải sửa constructor
- D. Chỉ dùng được với static class

**45.** Khi dependency thay đổi theo từng lần gọi (mỗi thao tác một dependency khác nhau), nên dùng pattern nào?

- A. Dependency cố định suốt vòng đời object
- B. Biến toàn cục
- C. Optional property
- D. Method Injection (truyền dependency như tham số của method)

## Chapter 04: Building Web Application using ASP.NET Core MVC

**46.** Đặc điểm của ASP.NET Core là gì?

- A. Chỉ chạy trên Windows và cần `System.Web.dll`
- B. Miễn phí, mã nguồn mở, đa nền tảng, kiến trúc modular nhẹ và không phụ thuộc `System.Web.dll`
- C. Thay thế hoàn toàn C#
- D. Chỉ chạy được trên IIS

**47.** Hạn chế của ASP.NET Web Forms dẫn đến việc cần framework mới là gì?

- A. Dễ test và kiểm soát HTML hoàn toàn
- B. Mô hình stateless đơn giản
- C. Chỉ chạy trên Linux
- D. Thiếu testability, mô hình stateful phức tạp, ít kiểm soát HTML sinh ra

**48.** Hai nhiệm vụ chính của Routing là gì?

- A. Ánh xạ request đến vào Controller Action; sinh ra các URL đi ra tương ứng với action
- B. Render view và lưu database
- C. Mã hóa URL và nén response
- D. Quản lý session và cookie

**49.** Trong mẫu `{controller=Home}/{action=Index}/{id?}`, dấu `?` ở `id?` nghĩa là gì?

- A. Tham số id bắt buộc
- B. Id luôn bằng null
- C. Tham số id là tùy chọn (optional)
- D. Id là tên controller

**50.** Vai trò của Controller trong MVC là gì?

- A. Chỉ render giao diện HTML
- B. Xử lý tương tác/request người dùng, xử lý logic cần thiết và trả response (điều phối luồng logic)
- C. Chỉ lưu dữ liệu vào database
- D. Chỉ chứa CSS và JavaScript

**51.** Trách nhiệm của View trong MVC là gì?

- A. Chứa business logic
- B. Truy cập database
- C. Xử lý request đến
- D. Chỉ render model cho người dùng, không nên chứa logic xử lý

**52.** ViewModel là gì?

- A. Class chứa các property chỉ cần cho view; là tùy chọn và giúp chỉ phơi bày dữ liệu cần thiết
- B. Class bắt buộc phải có trong mọi view
- C. Chính là DbContext
- D. Chính là Controller

**53.** Điều kiện của một Action method là gì?

- A. Có thể là private
- B. Có thể là protected
- C. Phải là public và không được là static
- D. Có thể là static

**54.** Nếu không tìm thấy view trong thư mục `Views/<TênController>`, framework sẽ tìm tiếp ở đâu?

- A. Báo lỗi ngay lập tức
- B. Thư mục `Views/Shared`
- C. Thư mục `wwwroot`
- D. Thư mục `Models`

**55.** Model Binding là gì?

- A. Mã hóa dữ liệu form
- B. Kiểm tra quyền truy cập
- C. Tạo bảng trong database
- D. Ánh xạ dữ liệu từ HTTP request (form, route, query string, body) vào tham số của action method

**56.** Model Validation diễn ra khi nào?

- A. Sau model binding và trước khi action thực thi; lỗi được lưu trong `ModelState`
- B. Sau khi action đã chạy xong
- C. Trước model binding
- D. Chỉ diễn ra ở client

**57.** Attribute `[Compare]` dùng để làm gì?

- A. Kiểm tra định dạng email
- B. Kiểm tra giá trị trong một khoảng
- C. Kiểm tra hai property trong model khớp nhau (ví dụ Password và ConfirmPassword)
- D. Kiểm tra định dạng URL

**58.** So sánh ViewData và ViewBag?

- A. ViewBag là dictionary key/value, ViewData là dynamic
- B. ViewData là dictionary key/value; ViewBag tương tự nhưng truy cập bằng dynamic property
- C. Hai thứ hoàn toàn không liên quan
- D. Chỉ ViewBag truyền được dữ liệu từ controller sang view

**59.** TempData dùng để làm gì?

- A. Chỉ tồn tại trong một view
- B. Lưu vĩnh viễn vào database
- C. Chỉ tồn tại ở phía client
- D. Lưu tạm, thường dùng cho thông điệp một lần/chuyển dữ liệu giữa các action; `TempData.Keep()` giữ lại dữ liệu sau khi render view

**60.** `HttpContext.Items` có phạm vi lưu trữ như thế nào?

- A. Chỉ trong một request, bị loại bỏ sau khi request xử lý xong
- B. Qua nhiều request của cùng một user
- C. Lưu trong cookie của trình duyệt
- D. Lưu trong Session

## Chapter 05: Building Websites Using ASP.NET Core Razor Pages

**61.** Razor Pages là gì?

- A. Framework chạy hoàn toàn ở client-side
- B. Framework chỉ dùng được trên Windows
- C. Framework server-side, page-focused để xây web động, dựa trên MVC, có sẵn từ .NET Core 2.0
- D. Thư viện thay thế ngôn ngữ C#

**62.** Một Razor Page kèm PageModel gồm hai file nào?

- A. `Demo2.cshtml` và `Demo2.cshtml.cs` (cùng tên, ánh xạ 1-1)
- B. `Demo2.razor` và `Demo2.cs`
- C. `Demo2.html` và `Demo2.js`
- D. `Demo2.cshtml` và `Demo2.json`

**63.** Quy ước đặt tên handler method trong Razor Pages là gì?

- A. `Get()`, `Post()`
- B. `HandleGet()`, `HandlePost()`
- C. `get()`, `post()`
- D. Tiền tố "On": `OnGet()`, `OnPost()`; `OnGet` và `OnGetAsync` được coi là cùng một handler

**64.** `_Layout.cshtml` dùng để làm gì?

- A. Khai báo directive toàn cục
- B. Làm template cho các content page tham chiếu nó (phần giao diện nhất quán của site)
- C. Chạy code sau mỗi content page
- D. Render partial page

**65.** `_ViewStart.cshtml` có vai trò gì?

- A. Khai báo các namespace toàn cục
- B. Tham gia routing như một trang bình thường
- C. Chứa code chạy sau code của content page trong cùng thư mục/thư mục con; chỗ tiện để chỉ định layout
- D. Lưu connection string

**66.** `_ViewImports.cshtml` dùng để làm gì?

- A. Tập trung các directive dùng chung (`@using`, `@addTagHelper`, `@inject`, `@model`...) cho các Razor page
- B. Làm template layout
- C. Quản lý Session
- D. Kết nối database

**67.** Đặc điểm của Partial Pages là gì?

- A. Có tham gia routing
- B. Bắt buộc phải có PageModel
- C. Thay thế hoàn toàn layout
- D. Không tham gia routing; render bằng `<partial name="_MenuPartial" />`

**68.** PageModel trong Razor Pages là gì?

- A. Triển khai pattern MVVM
- B. Triển khai Page Controller pattern (một page ↔ một controller), vừa là controller vừa là view model
- C. Triển khai Front Controller xử lý mọi request của site
- D. Chỉ là một ViewModel thuần dữ liệu

**69.** Khi nào nên dùng View Component thay vì partial page hay tag helper?

- A. Khi chỉ cần HTML tĩnh
- B. Khi không cần unit test
- C. Khi cần logic để lấy dữ liệu (gọi file, database, web service); lớp kế thừa `ViewComponent` và có method `Invoke`
- D. Khi muốn thay thế routing

**70.** Trong Razor Pages routing, file `Index.cshtml` ở thư mục gốc `Pages` truy cập được qua URL nào?

- A. Cả `/` và `/Index`
- B. Chỉ `/Index`
- C. Chỉ `/Home`
- D. Không truy cập được

**71.** Vì sao vẫn cần server-side validation dù đã có client-side validation?

- A. Client-side validation không tồn tại
- B. Server-side nhanh hơn nên thay thế client
- C. Server-side không cần DataAnnotation
- D. Client-side rất dễ bị vượt qua; server sẽ chạy các validation attribute và `ModelState.IsValid` trả về `false` nếu có lỗi

**72.** Attribute `[BindProperty]` trên một public property của PageModel có tác dụng gì?

- A. Gắn property vào một handler method
- B. Bind giá trị form được post lên vào property (tiện dùng giá trị ngoài handler, bind với tag helper)
- C. Tự động validate mọi dữ liệu
- D. Định nghĩa route cho trang

**73.** Tag Helpers trong Razor Pages là gì?

- A. Thư viện JavaScript để điều khiển DOM
- B. Công cụ sinh code database
- C. Thành phần tái sử dụng tự động sinh HTML cho các tag cụ thể; bật bằng `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`
- D. Loại file layout

**74.** Về Session state trong Razor Pages, phát biểu nào đúng?

- A. Không được bật mặc định, phải bật trong file Startup/cấu hình app; dùng cookie để liên kết các request thành một session
- B. Mặc định đã bật sẵn
- C. Không cần dùng cookie
- D. Chỉ lưu ở phía client

**75.** Điều gì xảy ra với request không qua HTTPS khi dùng attribute `RequireHttps`?

- A. Được chấp nhận bình thường
- B. Luôn trả về 404 Not Found
- C. Luôn bị redirect sang HTTPS dù dùng verb nào
- D. Request GET được redirect sang phiên bản HTTPS; các verb khác (ví dụ POST) nhận 403 Forbidden

## Chapter 06: Building a Web App with Blazor and ASP.NET Core

**76.** Tên "Blazor" được ghép từ hai từ nào?

- A. Block + Razor
- B. Browser + Razor
- C. Binary + Lazor
- D. Backend + Azure

**77.** Trong Blazor Server, các tương tác UI được xử lý thông qua gì?

- A. SignalR connection
- B. FTP
- C. SMTP
- D. Remote Desktop

**78.** Blazor WebAssembly phù hợp nhất với loại ứng dụng nào?

- A. Ứng dụng nội bộ cần kết nối liên tục tới server
- B. Ứng dụng chỉ chạy trên server
- C. Ứng dụng offline-capable hoặc public (tải lần đầu lớn nhưng sau đó không cần tương tác server)
- D. Ứng dụng không dùng C#

**79.** Một Blazor component thường được định nghĩa trong file nào và dùng thế nào?

- A. File `.cshtml`, dùng `@Component("MyComponent")`
- B. File `.cs`, dùng `new MyComponent()`
- C. File `.html`, dùng `<include MyComponent>`
- D. File `.razor`, dùng như một thẻ: `<MyComponent />`

**80.** Directive nào khai báo route `/todo` cho một component?

- A. `@route "/todo"`
- B. `@page "/todo"`
- C. `@url "/todo"`
- D. `@path "/todo"`

**81.** `<input @bind="newTodo" />` thực hiện loại binding nào?

- A. Two-way binding giữa ô input và biến `newTodo`
- B. Chỉ one-way (hiển thị giá trị)
- C. Không binding gì
- D. Chỉ import namespace

**82.** Attribute nào gắn method C# vào sự kiện click của button?

- A. `@click`
- B. `onpress`
- C. `@onclick`
- D. `@submit`

**83.** File `Routes.razor` trong dự án Blazor Web App có vai trò gì?

- A. Chứa connection string
- B. Khai báo namespace dùng chung
- C. Là entry point khởi động server
- D. Cấu hình Blazor router (còn `App.razor` là root component)

**84.** File `_Imports.razor` dùng để làm gì?

- A. Cấu hình router
- B. Khai báo các namespace thường dùng chung cho các component
- C. Chứa static files
- D. Là entry point của ứng dụng

**85.** Cascading Values (cascading parameters) trong Blazor dùng để làm gì?

- A. Chia sẻ dữ liệu giữa các nested component
- B. Xóa dữ liệu khỏi component
- C. Chuyển trang
- D. Biên dịch code C#

**86.** Dòng `@rendermode InteractiveServer` ở đầu component có tác dụng gì?

- A. Tắt hoàn toàn interactivity
- B. Bắt component chạy bằng WebAssembly
- C. Bật interactivity cho component theo chế độ Interactive Server
- D. Chỉ render HTML tƒ©nh

**87.** Lệnh nào áp dụng các migration lên database trong demo Movie (EF Core)?

- A. `dotnet ef migrations add "InitialDB"` (lệnh này chỉ tạo file migration)
- B. `dotnet new blazor`
- C. `dotnet run`
- D. `dotnet ef database update`

**88.** Attribute `[Column(TypeName = "decimal(18, 2)")]` trên property `Price` có tác dụng gì?

- A. Định dạng hiển thị trên giao diện
- B. Chỉ định kiểu cột trong database
- C. Bắt buộc người dùng phải nhập giá
- D. Đổi tên bảng

## Chapter 07: Real-Time Communication with SignalR

**89.** "Pushing" khác "Pulling" ở điểm nào?

- A. Push là được thông báo có gì mới, thay vì phải đi hỏi xem có gì mới (Pull)
- B. Push là client liên tục hỏi server
- C. Pull là server tự gửi dữ liệu xuống client
- D. Hai khái niệm giống nhau

**90.** Thứ tự fallback của các transport trong SignalR là gì?

- A. Long Polling → Server-Sent Events → WebSockets
- B. Server-Sent Events → WebSockets → Long Polling
- C. WebSockets → Server-Sent Events → Long Polling
- D. Long Polling → WebSockets → Server-Sent Events

**91.** Cách tạo một Hub trong SignalR là gì?

- A. Tạo class implement `IDisposable`
- B. Tạo static class
- C. Tạo interface `IHub`
- D. Khai báo class kế thừa từ `Hub` và thêm các public methods

**92.** Để cấu hình SignalR trong `Program.cs` cần những gì?

- A. Chỉ cần `MapHub`
- B. `AddSignalR()` đăng ký services; `MapHub<ChatHub>("/chatHub")` ánh xạ hub tới URL endpoint
- C. Chỉ cần cài thư viện JavaScript phía client
- D. Không cần cấu hình gì

**93.** `Clients.Others` gọi method trên đối tượng nào?

- A. Tất cả client đang kết nối trừ client đã gọi hub method
- B. Chỉ client đã gọi hub method
- C. Mọi client đang kết nối
- D. Chỉ một group

**94.** `Context.ConnectionId` trả về gì?

- A. Tên người dùng
- B. Địa chỉ IP
- C. ID duy nhất do SignalR gán cho mỗi connection
- D. Tên group

**95.** Cách khắc phục nhược điểm "magic string" của `SendAsync("ReceiveMessage", ...)` là gì?

- A. Dùng JSON thay cho MessagePack
- B. Dùng hub filter
- C. Dùng `Context.Items`
- D. Strongly typed hub với `Hub<T>` để tránh lỗi runtime do sai tên method

**96.** Attribute `[HubMethodName("SendMessageToUser")]` có tác dụng gì?

- A. Đổi tên class hub
- B. Chỉ định tên mà client dùng để gọi method, thay cho tên method .NET mặc định
- C. Ẩn method khỏi client
- D. Cấu hình group

**97.** Trong `OnDisconnectedAsync(Exception exception)`, tham số `exception` là `null` khi nào?

- A. Khi client ngắt kết nối có chủ đích (ví dụ gọi `connection.stop()`)
- B. Khi có lỗi mạng (network failure)
- C. Khi server bị crash
- D. Khi connection bị timeout

**98.** Phát biểu nào đúng về Hub filters?

- A. Chỉ có trước ASP.NET Core 3.1
- B. Chỉ chạy phía client
- C. Có từ ASP.NET Core 5.0, implement `IHubFilter` với `InvokeMethodAsync`; global filters chạy trước local filters
- D. Thay thế hoàn toàn class `Hub`

**99.** MessagePack so với JSON trong SignalR thì thế nào?

- A. Lớn hơn và chậm hơn JSON
- B. Dễ đọc bằng mắt thường hơn
- C. Hoàn toàn giống nhau
- D. Là binary serialization nhỏ gọn và nhanh hơn (ví dụ slide: JSON 27 byte, MessagePack 18 byte)

**100.** Trong lab Entity Framework + SignalR, vì sao sau `SaveChangesAsync()` ta gọi `Clients.All.SendAsync("LoadProducts")`?

- A. Để lưu dữ liệu vào database
- B. Để báo mọi client tải lại danh sách sản phẩm (client gọi `LoadProdData()` rồi vẽ lại bảng), giúp dữ liệu đồng bộ real-time
- C. Để tạo migration mới
- D. Để xóa cache của server

## Đáp án & giải thích

### Chapter 01: Networking

- **1. B** — Port là số nguyên không dấu 2 byte, giúp OS phân biệt tiến trình giao tiếp mạng.
- **2. C** — IPv4 = 4 byte (ví dụ 192.143.5.1), IPv6 = 16 byte.
- **3. D** — TCP connection-based, tin cậy nhờ acknowledge.
- **4. A** — UDP gửi datagram độc lập, không đảm bảo đến nơi/đúng thứ tự; app phải tự xử lý gói mất, trùng, sai thứ tự.
- **5. C** — URI là chuỗi mô tả tài nguyên; URL và URN là tập con.
- **6. B** — URN dùng scheme "urn", độc lập vị trí, không ngụ ý tài nguyên có sẵn. URL mới là thứ chỉ vị trí.
- **7. A** — WebRequest là abstract base class cho HTTP, FTP, `file:`.
- **8. D** — Client không tự tạo WebResponse, mà lấy qua `GetResponse()`.
- **9. B** — HttpClient hợp web API/REST, có custom message handlers (mock được khi unit test), mỗi instance có connection pool riêng.
- **10. C** — `GetStringAsync` gửi GET và trả body dạng string.
- **11. A** — DNS ánh xạ tên miền sang IP.
- **12. D** — Name Space, Globally Distributed Database (Name Servers), Resolver.
- **13. B** — TcpListener lắng nghe và accept kết nối; TcpClient là phía kết nối tới.
- **14. C** — `GetStream()` trả về `NetworkStream` để gửi/nhận.
- **15. A** — `Listen()` đặt Socket vào trạng thái lắng nghe; `Accept()` tạo Socket mới cho kết nối đến.

### Chapter 02: Async & Parallel

- **16. D** — SISD (single instruction, single data), gặp nút thắt von Neumann.
- **17. B** — SMP: processor/core nào rảnh cũng chạy được task. Câu A là AMP (asymmetrical).
- **18. C** — Quad-core x HT (2 logical core/core) = 8 task cùng lúc.
- **19. A** — Intel phát triển HT, ra mắt trên Xeon năm 2002.
- **20. B** — MIMD cho true parallelism, được dùng bởi hầu hết hệ thống hiện nay.
- **21. C** — MISD dùng cho fault tolerance, ví dụ máy tính điều khiển tàu con thoi.
- **22. A** — Serial: lệnh chạy tuần tự trên một processor.
- **23. D** — Giao tiếp và đồng bộ giữa sub-tasks khó.
- **24. B** — TPL nằm trong `System.Threading` và `System.Threading.Tasks`.
- **25. C** — `Task` không trả kết quả; cần `Task<TResult>`.
- **26. A** — `WhenAll` hoàn thành khi tất cả task con xong.
- **27. D** — `AsParallel()` là entry point của PLINQ.
- **28. B** — `AsOrdered` giữ thứ tự chuỗi nguồn; `AsUnordered` thì không cần giữ.
- **29. C** — Parallel có overhead, cần đo; tránh I/O không thread-safe; `AsParallel` không đảm bảo chạy song song.
- **30. A** — `async/await` giúp viết mã bất đồng bộ gần như đồng bộ; thread gọi không bị chặn.

### Chapter 03: Dependency Injection

- **31. C** — Robert C. Martin (2000); Michael Feathers đặt tên viết tắt SOLID.
- **32. A** — S: một class, một trách nhiệm.
- **33. D** — O: mở cho mở rộng, đóng với sửa đổi.
- **34. B** — L: subtype thay thế được mà không làm sai chương trình.
- **35. C** — I: nhiều interface chuyên biệt tốt hơn một interface chung.
- **36. A** — D: cả high-level và low-level đều phụ thuộc abstraction.
- **37. D** — `new Email()` trong constructor là tightly coupled, vi phạm DIP và OCP.
- **38. B** — DIP là nguyên tắc/lý thuyết; IoC là pattern có các bước cụ thể để code thân thiện với DIP.
- **39. C** — DI là cách hiện thực IoC: inject implementation vào class dùng interface.
- **40. A** — Registration (đăng ký), Resolution (tạo và inject), Disposition (quản lý vòng đời).
- **41. D** — Transient tạo mới mỗi lần yêu cầu; Singleton dùng một instance duy nhất; Scoped giới hạn trong một context.
- **42. B** — Ambient Context cung cấp dependency qua static accessor.
- **43. C** — Constructor Injection: phổ biến nhất, đảm bảo dependency có sẵn khi object được khởi tạo.
- **44. A** — Property Injection ẩn dependency và có thể null, dễ gây lỗi `NullReferenceException`.
- **45. D** — Method Injection: dependency khác nhau theo từng lần gọi, truyền bằng parameter.

### Chapter 04: ASP.NET Core MVC

- **46. B** — Free, open-source, cross-platform, modular, không còn phụ thuộc `System.Web.dll`.
- **47. D** — Web Forms: thiếu testability, stateful phức tạp, ít kiểm soát HTML.
- **48. A** — Routing map request → action và sinh URL đi ra.
- **49. C** — `?` = tham số optional.
- **50. B** — Controller điều phối luồng logic và trả response.
- **51. D** — View chỉ render, không xử lý logic.
- **52. A** — ViewModel chứa dữ liệu riêng cho view, là tùy chọn.
- **53. C** — Action method phải `public` và không `static`.
- **54. B** — View không thấy ở `Views/<Controller>` thì tìm tiếp ở `Views/Shared`.
- **55. D** — Model binding map dữ liệu request vào tham số action.
- **56. A** — Validation chạy sau binding, trước action; kết quả nằm trong `ModelState`.
- **57. C** — `[Compare]` so khớp hai property.
- **58. B** — ViewData là dictionary; ViewBag là wrapper dùng dynamic property.
- **59. D** — TempData cho dữ liệu tạm/một lần; `Keep()` giữ lại sau khi render.
- **60. A** — `HttpContext.Items` chỉ sống trong một request.

### Chapter 05: Razor Pages

- **61. C** — Server-side, page-focused, có từ .NET Core 2.0.
- **62. A** — `.cshtml` + `.cshtml.cs`, ánh xạ 1-1.
- **63. D** — Quy ước "On" + verb: `OnGet`, `OnPost`; bản async là cùng handler.
- **64. B** — `_Layout` là template chung cho các trang.
- **65. C** — `_ViewStart` chạy sau code content page, tiện để chỉ định layout.
- **66. A** — `_ViewImports` tập trung directive dùng chung.
- **67. D** — Partial page không tham gia routing; render bằng `<partial />`.
- **68. B** — PageModel = Page Controller pattern; không phải MVVM, không phải Front Controller.
- **69. C** — View Component dùng khi cần logic lấy dữ liệu; lớp kế thừa `ViewComponent`, có `Invoke`.
- **70. A** — `Index.cshtml` truy cập qua `/` và `/Index`.
- **71. D** — Client-side dễ bị vô hiệu hóa, nên server phải kiểm tra lại. Gợi ý cho Tester: thử tắt JavaScript rồi submit dữ liệu sai.
- **72. B** — `[BindProperty]` bind form value vào property của PageModel.
- **73. C** — Tag Helper tự sinh HTML cho tag cụ thể, bật bằng `@addTagHelper`.
- **74. A** — Session không bật mặc định và dùng cookie để gắn các request.
- **75. D** — GET → redirect sang HTTPS; verb khác như POST → 403 Forbidden.

### Chapter 06: Blazor

- **76. B** — Blazor = **B**rowser + **R**azor.
- **77. A** — Blazor Server xử lý UI qua SignalR connection.
- **78. C** — WASM hợp app offline-capable hoặc public; tải lần đầu nặng.
- **79. D** — Component nằm trong file `.razor`, dùng như `<MyComponent />`.
- **80. B** — `@page "/todo"` khai báo route.
- **81. A** — `@bind` là two-way binding.
- **82. C** — `@onclick` gắn method C# vào sự kiện click.
- **83. D** — `Routes.razor` cấu hình router; `App.razor` là root component.
- **84. B** — `_Imports.razor` khai báo namespace chung.
- **85. A** — Cascading parameters chia sẻ dữ liệu giữa nested components.
- **86. C** — `@rendermode InteractiveServer` bật interactivity kiểu Server.
- **87. D** — `database update` áp dụng migration; `migrations add` chỉ tạo file migration.
- **88. B** — `[Column(TypeName=...)]` quy định kiểu cột trong DB.

### Chapter 07: SignalR

- **89. A** — Push = được báo có gì mới; Pull = đi hỏi.
- **90. C** — WebSockets → Server-Sent Events → Long Polling.
- **91. D** — Hub là class kế thừa `Hub`, các public method là thứ client gọi được.
- **92. B** — `AddSignalR()` + `MapHub<T>(url)`.
- **93. A** — `Others` = mọi client trừ caller.
- **94. C** — `ConnectionId` là ID duy nhất của mỗi connection.
- **95. D** — `Hub<T>` giúp bắt lỗi sai tên ngay lúc biên dịch thay vì lỗi runtime.
- **96. B** — `[HubMethodName]` đổi tên method mà client gọi.
- **97. A** — `null` khi ngắt chủ động; có exception khi ngắt do lỗi như mạng.
- **98. C** — Hub filter từ ASP.NET Core 5.0, implement `IHubFilter`; global chạy trước local.
- **99. D** — MessagePack nhỏ và nhanh hơn JSON (18 byte so với 27 byte trong ví dụ).
- **100. B** — Báo mọi client tải lại danh sách để dữ liệu đồng bộ real-time.
