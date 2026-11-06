Port là gì?
A. Địa chỉ 4 byte định danh một máy tính
B. Số nguyên không dấu 2 byte giúp hệ điều hành phân biệt các tiến trình giao tiếp mạng
C. Tên miền (hostname) của server
D. Giao thức đóng gói dữ liệu
Đáp án: B
Giải thích: Port là số nguyên không dấu 2 byte, giúp OS phân biệt tiến trình giao tiếp mạng.

Độ dài của địa chỉ IPv4 và IPv6 lần lượt là?
A. 2 byte và 8 byte
B. 8 byte và 16 byte
C. 4 byte và 16 byte
D. 4 byte và 32 byte
Đáp án: C
Giải thích: IPv4 = 4 byte (ví dụ 192.143.5.1), IPv6 = 16 byte.

Đặc điểm của TCP (Transmission Control Protocol) là gì?
A. Connectionless, không có cơ chế xác nhận
B. Gửi các datagram độc lập, không đảm bảo đến nơi
C. Không cần thiết lập kết nối trước khi gửi
D. Connection-based, đảm bảo luồng dữ liệu tin cậy nhờ cơ chế acknowledge
Đáp án: D
Giải thích: TCP connection-based, tin cậy nhờ acknowledge.

Phát biểu nào đúng về UDP (User Datagram Protocol)?
A. Gửi các datagram độc lập, không đảm bảo đến nơi hay đúng thứ tự
B. Luôn đảm bảo đúng thứ tự gói tin
C. Dùng cơ chế acknowledge để xác nhận
D. Chỉ dùng duy nhất một đường kết nối
Đáp án: A
Giải thích: UDP gửi datagram độc lập, không đảm bảo đến nơi/đúng thứ tự; app phải tự xử lý gói mất, trùng, sai thứ tự.

URI (Uniform Resource Identifier) là gì?
A. Chỉ là địa chỉ IP của server
B. Đường dẫn vật lý của file trên ổ đĩa
C. Chuỗi định dạng đặc biệt mô tả một tài nguyên trên internet hoặc LAN (URL và URN là tập con của URI)
D. Giao thức truyền file
Đáp án: C
Giải thích: URI là chuỗi mô tả tài nguyên; URL và URN là tập con.

Đặc điểm của URN (ví dụ `urn:isbn:0451450523`) là gì?
A. Cho biết vị trí mạng nơi tài nguyên nằm
B. Là URI dùng scheme "urn", định danh độc lập vị trí và không ngụ ý tài nguyên có sẵn để truy cập
C. Luôn bắt đầu bằng `http`
D. Đảm bảo tài nguyên luôn truy cập được
Đáp án: B
Giải thích: URN dùng scheme "urn", độc lập vị trí, không ngụ ý tài nguyên có sẵn. URL mới là thứ chỉ vị trí.

Lớp nào là abstract base class của mô hình request/response (HTTP, FTP, `file:`) theo kiểu không phụ thuộc giao thức?
A. WebRequest
B. HttpClient
C. TcpClient
D. Dns
Đáp án: A
Giải thích: WebRequest là abstract base class cho HTTP, FTP, `file:`.

Client lấy được đối tượng `WebResponse` bằng cách nào?
A. `new WebResponse()`
B. `WebRequest.Create(Uri)`
C. `WebRequest.Abort()`
D. Gọi `GetResponse()` trên một `WebRequest`
Đáp án: D
Giải thích: Client không tự tạo WebResponse, mà lấy qua `GetResponse()`.

HttpClient khác WebRequest/WebClient ở điểm nào?
A. Chỉ dùng cho giao thức FTP
B. Thiết kế cho web API/REST, cho phép plug custom message handlers (mock khi unit test), mỗi instance có connection pool riêng
C. Không hỗ trợ lập trình bất đồng bộ
D. Bắt buộc dựa trên WebRequest trong mọi phiên bản .NET
Đáp án: B
Giải thích: HttpClient hợp web API/REST, có custom message handlers (mock được khi unit test), mỗi instance có connection pool riêng.

Method nào gửi GET và trả về response body dạng `string` theo kiểu bất đồng bộ?
A. `PostAsync`
B. `DeleteAsync`
C. `GetStringAsync`
D. `PutAsync`
Đáp án: C
Giải thích: `GetStringAsync` gửi GET và trả body dạng string.

Domain Name System (DNS) làm nhiệm vụ gì?
A. Chuyển địa chỉ Internet dạng gợi nhớ (tên miền) thành địa chỉ IP tương ứng
B. Mã hóa dữ liệu truyền đi
C. Cấp phát số port cho tiến trình
D. Gửi email qua SMTP
Đáp án: A
Giải thích: DNS ánh xạ tên miền sang IP.

Ba thành phần của DNS là gì?
A. IP, Port, Protocol
B. Client, Server, Router
C. Socket, Stream, Listener
D. Name Space, Globally Distributed Database (Name Servers), Resolver
Đáp án: D
Giải thích: Name Space, Globally Distributed Database (Name Servers), Resolver.

Lớp nào phía server dùng để lắng nghe và chấp nhận kết nối TCP đến?
A. TcpClient
B. TcpListener
C. UdpClient
D. NetworkStream
Đáp án: B
Giải thích: TcpListener lắng nghe và accept kết nối; TcpClient là phía kết nối tới.

Method nào của `TcpClient` trả về `NetworkStream` để gửi/nhận dữ liệu?
A. `Accept()`
B. `Listen()`
C. `GetStream()`
D. `Abort()`
Đáp án: C
Giải thích: `GetStream()` trả về `NetworkStream` để gửi/nhận.

Method nào của lớp `Socket` đặt socket vào trạng thái lắng nghe?
A. `Listen()`
B. `Connect(IPAddress, Int32)`
C. `Send(Byte[])`
D. `Close()`
Đáp án: A
Giải thích: `Listen()` đặt Socket vào trạng thái lắng nghe; `Accept()` tạo Socket mới cho kết nối đến.

Hệ mono-processor kiểu von Neumann (input-processing-output) còn được gọi là mô hình nào trong Flynn's Taxonomy?
A. MIMD
B. SIMD
C. MISD
D. SISD
Đáp án: D
Giải thích: SISD (single instruction, single data), gặp nút thắt von Neumann.

Symmetrical multiprocessing (SMP) có nghĩa là gì?
A. Một processor chính làm manager phân phát task cho các processor khác
B. Bất kỳ processor/core rảnh nào cũng có thể thực thi task
C. Chỉ dùng được một core
D. Chỉ chạy được phần mềm tối ưu cho đa xử lý
Đáp án: B
Giải thích: SMP: processor/core nào rảnh cũng chạy được task. Câu A là AMP (asymmetrical).

Một processor có chip quad-core bật Hyper-Threading (HT) chạy được tối đa bao nhiêu task cùng lúc?
A. 4
B. 2
C. 8
D. 16
Đáp án: C
Giải thích: Quad-core x HT (2 logical core/core) = 8 task cùng lúc.

Công nghệ Hyper-Threading do hãng nào phát triển (ra mắt trên Xeon năm 2002)?
A. Intel
B. AMD
C. ARM
D. IBM
Đáp án: A
Giải thích: Intel phát triển HT, ra mắt trên Xeon năm 2002.

Kiến trúc nào cho phép true parallelism và được hầu hết hệ thống hiện nay sử dụng?
A. SISD
B. MIMD
C. SIMD
D. MISD
Đáp án: B
Giải thích: MIMD cho true parallelism, được dùng bởi hầu hết hệ thống hiện nay.

Kiến trúc nào thường dùng cho fault tolerance (chịu lỗi), ví dụ máy tính điều khiển tàu con thoi?
A. SISD
B. SIMD
C. MISD
D. MIMD
Đáp án: C
Giải thích: MISD dùng cho fault tolerance, ví dụ máy tính điều khiển tàu con thoi.

Đặc điểm của Serial Computing (tính toán tuần tự) là gì?
A. Các lệnh chạy lần lượt trên một processor, tại một thời điểm chỉ một lệnh được thực thi
B. Bài toán chia thành các phần chạy đồng thời
C. Dùng nhiều processor cùng lúc
D. Cần cơ chế điều phối trung tâm
Đáp án: A
Giải thích: Serial: lệnh chạy tuần tự trên một processor.

Hạn chế của Parallel Computing là gì?
A. Luôn tốn nhiều chi phí hơn serial
B. Không dùng được cho mô phỏng thời gian thực
C. Chỉ chạy được trên một CPU
D. Giao tiếp và đồng bộ (synchronization) giữa các sub-tasks khó thực hiện
Đáp án: D
Giải thích: Giao tiếp và đồng bộ giữa sub-tasks khó.

TPL (Task Parallel Library) nằm trong các namespace nào?
A. `System.Net`
B. `System.Threading` và `System.Threading.Tasks`
C. `System.Linq`
D. `System.IO`
Đáp án: B
Giải thích: TPL nằm trong `System.Threading` và `System.Threading.Tasks`.

Muốn một task trả về giá trị, ta dùng lớp nào?
A. `Task`
B. `TaskFactory`
C. `Task<TResult>`
D. `TaskScheduler`
Đáp án: C
Giải thích: `Task` không trả kết quả; cần `Task<TResult>`.

`Task.WhenAll(Task[])` làm gì?
A. Tạo task hoàn thành khi tất cả task trong mảng đã hoàn thành
B. Hoàn thành ngay khi task đầu tiên xong
C. Hủy tất cả các task
D. Chạy các task lần lượt tuần tự
Đáp án: A
Giải thích: `WhenAll` hoàn thành khi tất cả task con xong.

Dùng method nào để chuyển một truy vấn LINQ sang PLINQ?
A. `AsSequential()`
B. `AsOrdered()`
C. `ForAll()`
D. `AsParallel()`
Đáp án: D
Giải thích: `AsParallel()` là entry point của PLINQ.

Operator `AsOrdered` của PLINQ có tác dụng gì?
A. Bỏ qua thứ tự của nguồn dữ liệu
B. Yêu cầu PLINQ giữ nguyên thứ tự của chuỗi nguồn
C. Giới hạn số processor tối đa
D. Bắt query chạy tuần tự
Đáp án: B
Giải thích: `AsOrdered` giữ thứ tự chuỗi nguồn; `AsUnordered` thì không cần giữ.

Lưu ý đúng khi dùng PLINQ là gì?
A. PLINQ luôn nhanh hơn LINQ
B. Nên ghi file/database trực tiếp bên trong query PLINQ
C. Parallel có overhead nên cần đo hiệu năng; `AsParallel()` không đảm bảo chắc chắn chạy song song
D. PLINQ luôn chạy song song khi có `AsParallel()`
Đáp án: C
Giải thích: Parallel có overhead, cần đo; tránh I/O không thread-safe; `AsParallel` không đảm bảo chạy song song.

`async` và `await` giúp được gì?
A. Viết async method gần như dễ như synchronous, thread gọi không bị chặn chờ
B. Bắt buộc tạo thread mới cho mỗi lệnh
C. Chỉ chạy được trên Windows
D. Thay thế hoàn toàn lớp `Task`
Đáp án: A
Giải thích: `async/await` giúp viết mã bất đồng bộ gần như đồng bộ; thread gọi không bị chặn.

Các nguyên lý SOLID do ai phát triển?
A. Bill Gates
B. Anders Hejlsberg
C. Robert C. Martin (bài luận năm 2000); tên viết tắt SOLID do Michael Feathers đặt
D. Martin Fowler
Đáp án: C
Giải thích: Robert C. Martin (2000); Michael Feathers đặt tên viết tắt SOLID.

Chữ S (Single Responsibility Principle) nói gì?
A. Một class chỉ nên có một trách nhiệm (một lý do để thay đổi)
B. Mở cho mở rộng, đóng với sửa đổi
C. Subtype thay thế được type cha
D. Nhiều interface chuyên biệt tốt hơn một interface chung
Đáp án: A
Giải thích: S: một class, một trách nhiệm.

Chữ O (Open/Closed Principle) nói gì?
A. Một class chỉ có một trách nhiệm
B. Subtype thay thế được type cha
C. Module cấp cao phụ thuộc vào abstraction
D. Software entity mở cho mở rộng (extension), đóng với sửa đổi (modification)
Đáp án: D
Giải thích: O: mở cho mở rộng, đóng với sửa đổi.

Chữ L (Liskov Substitution Principle) nói gì?
A. Nhiều interface nhỏ tốt hơn một interface lớn
B. Đối tượng có thể được thay bằng instance của subtype mà không làm sai tính đúng đắn của chương trình
C. Class chỉ có một trách nhiệm
D. Details phải phụ thuộc vào abstractions
Đáp án: B
Giải thích: L: subtype thay thế được mà không làm sai chương trình.

Chữ I (Interface Segregation Principle) nói gì?
A. Dùng một interface chung thật lớn cho mọi client
B. Không nên dùng interface
C. Nhiều interface chuyên biệt cho từng client tốt hơn một interface đa dụng
D. High-level module phụ thuộc low-level module
Đáp án: C
Giải thích: I: nhiều interface chuyên biệt tốt hơn một interface chung.

Chữ D (Dependency Inversion Principle) nói gì?
A. High-level module không nên phụ thuộc low-level module; cả hai nên phụ thuộc vào abstractions
B. Low-level module phải phụ thuộc vào high-level module
C. Abstractions phụ thuộc vào details
D. Không dùng interface trong thiết kế
Đáp án: A
Giải thích: D: cả high-level và low-level đều phụ thuộc abstraction.

Class `Notification` tạo trực tiếp `new Email()` trong constructor. Đây là dạng gì?
A. Loosely coupled
B. Tuân thủ Dependency Inversion Principle
C. Tuân thủ Open/Closed Principle
D. Tightly coupled: muốn gửi loại thông báo khác thì phải sửa lại `Notification`
Đáp án: D
Giải thích: `new Email()` trong constructor là tightly coupled, vi phạm DIP và OCP.

Quan hệ giữa DIP và IoC (Inversion of Control) là gì?
A. Hai khái niệm giống hệt nhau
B. DIP là nguyên tắc (lý thuyết); IoC là pattern với các bước cụ thể để code thân thiện với DIP
C. IoC là một framework của Microsoft
D. DIP là pattern, IoC là nguyên tắc
Đáp án: B
Giải thích: DIP là nguyên tắc/lý thuyết; IoC là pattern có các bước cụ thể để code thân thiện với DIP.

Dependency Injection (DI) là gì?
A. Tạo object bằng `new` bên trong class sử dụng nó
B. Dùng static class cho mọi dependency
C. Cách thực hiện IoC bằng việc inject implementation cụ thể vào class đang dùng abstraction (interface)
D. Một loại interface đặc biệt
Đáp án: C
Giải thích: DI là cách hiện thực IoC: inject implementation vào class dùng interface.

Ba tính năng cơ bản của DI Container là gì?
A. Registration, Resolution, Disposition
B. Compile, Link, Run
C. Create, Read, Update
D. Connect, Send, Receive
Đáp án: A
Giải thích: Registration (đăng ký), Resolution (tạo và inject), Disposition (quản lý vòng đời).

Service lifetime nào tạo instance mới mỗi lần được yêu cầu?
A. Singleton
B. Scoped
C. Static
D. Transient
Đáp án: D
Giải thích: Transient tạo mới mỗi lần yêu cầu; Singleton dùng một instance duy nhất; Scoped giới hạn trong một context.

Ambient Context cung cấp dependency bằng cách nào?
A. Truyền qua constructor
B. Qua một static accessor, tránh làm rối API bởi các cross-cutting concerns
C. Truyền qua method parameter
D. Qua writable property
Đáp án: B
Giải thích: Ambient Context cung cấp dependency qua static accessor.

Pattern DI nào đảm bảo dependency luôn có sẵn vì mọi caller bắt buộc cung cấp nó qua constructor?
A. Property Injection
B. Method Injection
C. Constructor Injection
D. Ambient Context
Đáp án: C
Giải thích: Constructor Injection: phổ biến nhất, đảm bảo dependency có sẵn khi object được khởi tạo.

Nhược điểm của Property Injection là gì?
A. Ẩn dependency và property có thể null nên cần kiểm tra null ở mọi nơi dùng
B. Đảm bảo dependency luôn có khi khởi tạo
C. Bắt buộc phải sửa constructor
D. Chỉ dùng được với static class
Đáp án: A
Giải thích: Property Injection ẩn dependency và có thể null, dễ gây lỗi `NullReferenceException`.

Khi dependency thay đổi theo từng lần gọi (mỗi thao tác một dependency khác nhau), nên dùng pattern nào?
A. Dependency cố định suốt vòng đời object
B. Biến toàn cục
C. Optional property
D. Method Injection (truyền dependency như tham số của method)
Đáp án: D
Giải thích: Method Injection: dependency khác nhau theo từng lần gọi, truyền bằng parameter.

Đặc điểm của ASP.NET Core là gì?
A. Chỉ chạy trên Windows và cần `System.Web.dll`
B. Miễn phí, mã nguồn mở, đa nền tảng, kiến trúc modular nhẹ và không phụ thuộc `System.Web.dll`
C. Thay thế hoàn toàn C#
D. Chỉ chạy được trên IIS
Đáp án: B
Giải thích: Free, open-source, cross-platform, modular, không còn phụ thuộc `System.Web.dll`.

Hạn chế của ASP.NET Web Forms dẫn đến việc cần framework mới là gì?
A. Dễ test và kiểm soát HTML hoàn toàn
B. Mô hình stateless đơn giản
C. Chỉ chạy trên Linux
D. Thiếu testability, mô hình stateful phức tạp, ít kiểm soát HTML sinh ra
Đáp án: D
Giải thích: Web Forms: thiếu testability, stateful phức tạp, ít kiểm soát HTML.

Hai nhiệm vụ chính của Routing là gì?
A. Ánh xạ request đến vào Controller Action; sinh ra các URL đi ra tương ứng với action
B. Render view và lưu database
C. Mã hóa URL và nén response
D. Quản lý session và cookie
Đáp án: A
Giải thích: Routing map request → action và sinh URL đi ra.

Trong mẫu `{controller=Home}/{action=Index}/{id?}`, dấu `?` ở `id?` nghĩa là gì?
A. Tham số id bắt buộc
B. Id luôn bằng null
C. Tham số id là tùy chọn (optional)
D. Id là tên controller
Đáp án: C
Giải thích: `?` = tham số optional.

Vai trò của Controller trong MVC là gì?
A. Chỉ render giao diện HTML
B. Xử lý tương tác/request người dùng, xử lý logic cần thiết và trả response (điều phối luồng logic)
C. Chỉ lưu dữ liệu vào database
D. Chỉ chứa CSS và JavaScript
Đáp án: B
Giải thích: Controller điều phối luồng logic và trả response.

Trách nhiệm của View trong MVC là gì?
A. Chứa business logic
B. Truy cập database
C. Xử lý request đến
D. Chỉ render model cho người dùng, không nên chứa logic xử lý
Đáp án: D
Giải thích: View chỉ render, không xử lý logic.

ViewModel là gì?
A. Class chứa các property chỉ cần cho view; là tùy chọn và giúp chỉ phơi bày dữ liệu cần thiết
B. Class bắt buộc phải có trong mọi view
C. Chính là DbContext
D. Chính là Controller
Đáp án: A
Giải thích: ViewModel chứa dữ liệu riêng cho view, là tùy chọn.

Điều kiện của một Action method là gì?
A. Có thể là private
B. Có thể là protected
C. Phải là public và không được là static
D. Có thể là static
Đáp án: C
Giải thích: Action method phải `public` và không `static`.

Nếu không tìm thấy view trong thư mục `Views/<TênController>`, framework sẽ tìm tiếp ở đâu?
A. Báo lỗi ngay lập tức
B. Thư mục `Views/Shared`
C. Thư mục `wwwroot`
D. Thư mục `Models`
Đáp án: B
Giải thích: View không thấy ở `Views/<Controller>` thì tìm tiếp ở `Views/Shared`.

Model Binding là gì?
A. Mã hóa dữ liệu form
B. Kiểm tra quyền truy cập
C. Tạo bảng trong database
D. Ánh xạ dữ liệu từ HTTP request (form, route, query string, body) vào tham số của action method
Đáp án: D
Giải thích: Model binding map dữ liệu request vào tham số action.

Model Validation diễn ra khi nào?
A. Sau model binding và trước khi action thực thi; lỗi được lưu trong `ModelState`
B. Sau khi action đã chạy xong
C. Trước model binding
D. Chỉ diễn ra ở client
Đáp án: A
Giải thích: Validation chạy sau binding, trước action; kết quả nằm trong `ModelState`.

Attribute `[Compare]` dùng để làm gì?
A. Kiểm tra định dạng email
B. Kiểm tra giá trị trong một khoảng
C. Kiểm tra hai property trong model khớp nhau (ví dụ Password và ConfirmPassword)
D. Kiểm tra định dạng URL
Đáp án: C
Giải thích: `[Compare]` so khớp hai property.

So sánh ViewData và ViewBag?
A. ViewBag là dictionary key/value, ViewData là dynamic
B. ViewData là dictionary key/value; ViewBag tương tự nhưng truy cập bằng dynamic property
C. Hai thứ hoàn toàn không liên quan
D. Chỉ ViewBag truyền được dữ liệu từ controller sang view
Đáp án: B
Giải thích: ViewData là dictionary; ViewBag là wrapper dùng dynamic property.

TempData dùng để làm gì?
A. Chỉ tồn tại trong một view
B. Lưu vĩnh viễn vào database
C. Chỉ tồn tại ở phía client
D. Lưu tạm, thường dùng cho thông điệp một lần/chuyển dữ liệu giữa các action; `TempData.Keep()` giữ lại dữ liệu sau khi render view
Đáp án: D
Giải thích: TempData cho dữ liệu tạm/một lần; `Keep()` giữ lại sau khi render.

`HttpContext.Items` có phạm vi lưu trữ như thế nào?
A. Chỉ trong một request, bị loại bỏ sau khi request xử lý xong
B. Qua nhiều request của cùng một user
C. Lưu trong cookie của trình duyệt
D. Lưu trong Session
Đáp án: A
Giải thích: `HttpContext.Items` chỉ sống trong một request.

Razor Pages là gì?
A. Framework chạy hoàn toàn ở client-side
B. Framework chỉ dùng được trên Windows
C. Framework server-side, page-focused để xây web động, dựa trên MVC, có sẵn từ .NET Core 2.0
D. Thư viện thay thế ngôn ngữ C#
Đáp án: C
Giải thích: Server-side, page-focused, có từ .NET Core 2.0.

Một Razor Page kèm PageModel gồm hai file nào?
A. `Demo2.cshtml` và `Demo2.cshtml.cs` (cùng tên, ánh xạ 1-1)
B. `Demo2.razor` và `Demo2.cs`
C. `Demo2.html` và `Demo2.js`
D. `Demo2.cshtml` và `Demo2.json`
Đáp án: A
Giải thích: `.cshtml` + `.cshtml.cs`, ánh xạ 1-1.

Quy ước đặt tên handler method trong Razor Pages là gì?
A. `Get()`, `Post()`
B. `HandleGet()`, `HandlePost()`
C. `get()`, `post()`
D. Tiền tố "On": `OnGet()`, `OnPost()`; `OnGet` và `OnGetAsync` được coi là cùng một handler
Đáp án: D
Giải thích: Quy ước "On" + verb: `OnGet`, `OnPost`; bản async là cùng handler.

`_Layout.cshtml` dùng để làm gì?
A. Khai báo directive toàn cục
B. Làm template cho các content page tham chiếu nó (phần giao diện nhất quán của site)
C. Chạy code sau mỗi content page
D. Render partial page
Đáp án: B
Giải thích: `_Layout` là template chung cho các trang.

`_ViewStart.cshtml` có vai trò gì?
A. Khai báo các namespace toàn cục
B. Tham gia routing như một trang bình thường
C. Chứa code chạy sau code của content page trong cùng thư mục/thư mục con; chỗ tiện để chỉ định layout
D. Lưu connection string
Đáp án: C
Giải thích: `_ViewStart` chạy sau code content page, tiện để chỉ định layout.

`_ViewImports.cshtml` dùng để làm gì?
A. Tập trung các directive dùng chung (`@using`, `@addTagHelper`, `@inject`, `@model`...) cho các Razor page
B. Làm template layout
C. Quản lý Session
D. Kết nối database
Đáp án: A
Giải thích: `_ViewImports` tập trung directive dùng chung.

Đặc điểm của Partial Pages là gì?
A. Có tham gia routing
B. Bắt buộc phải có PageModel
C. Thay thế hoàn toàn layout
D. Không tham gia routing; render bằng `<partial name="_MenuPartial" />`
Đáp án: D
Giải thích: Partial page không tham gia routing; render bằng `<partial />`.

PageModel trong Razor Pages là gì?
A. Triển khai pattern MVVM
B. Triển khai Page Controller pattern (một page ↔ một controller), vừa là controller vừa là view model
C. Triển khai Front Controller xử lý mọi request của site
D. Chỉ là một ViewModel thuần dữ liệu
Đáp án: B
Giải thích: PageModel = Page Controller pattern; không phải MVVM, không phải Front Controller.

Khi nào nên dùng View Component thay vì partial page hay tag helper?
A. Khi chỉ cần HTML tĩnh
B. Khi không cần unit test
C. Khi cần logic để lấy dữ liệu (gọi file, database, web service); lớp kế thừa `ViewComponent` và có method `Invoke`
D. Khi muốn thay thế routing
Đáp án: C
Giải thích: View Component dùng khi cần logic lấy dữ liệu; lớp kế thừa `ViewComponent`, có `Invoke`.

Trong Razor Pages routing, file `Index.cshtml` ở thư mục gốc `Pages` truy cập được qua URL nào?
A. Cả `/` và `/Index`
B. Chỉ `/Index`
C. Chỉ `/Home`
D. Không truy cập được
Đáp án: A
Giải thích: `Index.cshtml` truy cập qua `/` và `/Index`.

Vì sao vẫn cần server-side validation dù đã có client-side validation?
A. Client-side validation không tồn tại
B. Server-side nhanh hơn nên thay thế client
C. Server-side không cần DataAnnotation
D. Client-side rất dễ bị vượt qua; server sẽ chạy các validation attribute và `ModelState.IsValid` trả về `false` nếu có lỗi
Đáp án: D
Giải thích: Client-side dễ bị vô hiệu hóa, nên server phải kiểm tra lại. Gợi ý cho Tester: thử tắt JavaScript rồi submit dữ liệu sai.

Attribute `[BindProperty]` trên một public property của PageModel có tác dụng gì?
A. Gắn property vào một handler method
B. Bind giá trị form được post lên vào property (tiện dùng giá trị ngoài handler, bind với tag helper)
C. Tự động validate mọi dữ liệu
D. Định nghĩa route cho trang
Đáp án: B
Giải thích: `[BindProperty]` bind form value vào property của PageModel.

Tag Helpers trong Razor Pages là gì?
A. Thư viện JavaScript để điều khiển DOM
B. Công cụ sinh code database
C. Thành phần tái sử dụng tự động sinh HTML cho các tag cụ thể; bật bằng `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`
D. Loại file layout
Đáp án: C
Giải thích: Tag Helper tự sinh HTML cho tag cụ thể, bật bằng `@addTagHelper`.

Về Session state trong Razor Pages, phát biểu nào đúng?
A. Không được bật mặc định, phải bật trong file Startup/cấu hình app; dùng cookie để liên kết các request thành một session
B. Mặc định đã bật sẵn
C. Không cần dùng cookie
D. Chỉ lưu ở phía client
Đáp án: A
Giải thích: Session không bật mặc định và dùng cookie để gắn các request.

Điều gì xảy ra với request không qua HTTPS khi dùng attribute `RequireHttps`?
A. Được chấp nhận bình thường
B. Luôn trả về 404 Not Found
C. Luôn bị redirect sang HTTPS dù dùng verb nào
D. Request GET được redirect sang phiên bản HTTPS; các verb khác (ví dụ POST) nhận 403 Forbidden
Đáp án: D
Giải thích: GET → redirect sang HTTPS; verb khác như POST → 403 Forbidden.

Tên "Blazor" được ghép từ hai từ nào?
A. Block + Razor
B. Browser + Razor
C. Binary + Lazor
D. Backend + Azure
Đáp án: B
Giải thích: Blazor = Browser + Razor.

Trong Blazor Server, các tương tác UI được xử lý thông qua gì?
A. SignalR connection
B. FTP
C. SMTP
D. Remote Desktop
Đáp án: A
Giải thích: Blazor Server xử lý UI qua SignalR connection.

Blazor WebAssembly phù hợp nhất với loại ứng dụng nào?
A. Ứng dụng nội bộ cần kết nối liên tục tới server
B. Ứng dụng chỉ chạy trên server
C. Ứng dụng offline-capable hoặc public (tải lần đầu lớn nhưng sau đó không cần tương tác server)
D. Ứng dụng không dùng C#
Đáp án: C
Giải thích: WASM hợp app offline-capable hoặc public; tải lần đầu nặng.

Một Blazor component thường được định nghĩa trong file nào và dùng thế nào?
A. File `.cshtml`, dùng `@Component("MyComponent")`
B. File `.cs`, dùng `new MyComponent()`
C. File `.html`, dùng `<include MyComponent>`
D. File `.razor`, dùng như một thẻ: `<MyComponent />`
Đáp án: D
Giải thích: Component nằm trong file `.razor`, dùng như `<MyComponent />`.

Directive nào khai báo route `/todo` cho một component?
A. `@route "/todo"`
B. `@page "/todo"`
C. `@url "/todo"`
D. `@path "/todo"`
Đáp án: B
Giải thích: `@page "/todo"` khai báo route.

`<input @bind="newTodo" />` thực hiện loại binding nào?
A. Two-way binding giữa ô input và biến `newTodo`
B. Chỉ one-way (hiển thị giá trị)
C. Không binding gì
D. Chỉ import namespace
Đáp án: A
Giải thích: `@bind` là two-way binding.

Attribute nào gắn method C# vào sự kiện click của button?
A. `@click`
B. `onpress`
C. `@onclick`
D. `@submit`
Đáp án: C
Giải thích: `@onclick` gắn method C# vào sự kiện click.

File `Routes.razor` trong dự án Blazor Web App có vai trò gì?
A. Chứa connection string
B. Khai báo namespace dùng chung
C. Là entry point khởi động server
D. Cấu hình Blazor router (còn `App.razor` là root component)
Đáp án: D
Giải thích: `Routes.razor` cấu hình router; `App.razor` là root component.

File `_Imports.razor` dùng để làm gì?
A. Cấu hình router
B. Khai báo các namespace thường dùng chung cho các component
C. Chứa static files
D. Là entry point của ứng dụng
Đáp án: B
Giải thích: `_Imports.razor` khai báo namespace chung.

Cascading Values (cascading parameters) trong Blazor dùng để làm gì?
A. Chia sẻ dữ liệu giữa các nested component
B. Xóa dữ liệu khỏi component
C. Chuyển trang
D. Biên dịch code C#
Đáp án: A
Giải thích: Cascading parameters chia sẻ dữ liệu giữa nested components.

Dòng `@rendermode InteractiveServer` ở đầu component có tác dụng gì?
A. Tắt hoàn toàn interactivity
B. Bắt component chạy bằng WebAssembly
C. Bật interactivity cho component theo chế độ Interactive Server
D. Chỉ render HTML tĩnh
Đáp án: C
Giải thích: `@rendermode InteractiveServer` bật interactivity kiểu Server.

Lệnh nào áp dụng các migration lên database trong demo Movie (EF Core)?
A. `dotnet ef migrations add "InitialDB"` (lệnh này chỉ tạo file migration)
B. `dotnet new blazor`
C. `dotnet run`
D. `dotnet ef database update`
Đáp án: D
Giải thích: `database update` áp dụng migration; `migrations add` chỉ tạo file migration.

Attribute `[Column(TypeName = "decimal(18, 2)")]` trên property `Price` có tác dụng gì?
A. Định dạng hiển thị trên giao diện
B. Chỉ định kiểu cột trong database
C. Bắt buộc người dùng phải nhập giá
D. Đổi tên bảng
Đáp án: B
Giải thích: `[Column(TypeName=...)]` quy định kiểu cột trong DB.

"Pushing" khác "Pulling" ở điểm nào?
A. Push là được thông báo có gì mới, thay vì phải đi hỏi xem có gì mới (Pull)
B. Push là client liên tục hỏi server
C. Pull là server tự gửi dữ liệu xuống client
D. Hai khái niệm giống nhau
Đáp án: A
Giải thích: Push = được báo có gì mới; Pull = đi hỏi.

Thứ tự fallback của các transport trong SignalR là gì?
A. Long Polling → Server-Sent Events → WebSockets
B. Server-Sent Events → WebSockets → Long Polling
C. WebSockets → Server-Sent Events → Long Polling
D. Long Polling → WebSockets → Server-Sent Events
Đáp án: C
Giải thích: WebSockets → Server-Sent Events → Long Polling.

Cách tạo một Hub trong SignalR là gì?
A. Tạo class implement `IDisposable`
B. Tạo static class
C. Tạo interface `IHub`
D. Khai báo class kế thừa từ `Hub` và thêm các public methods
Đáp án: D
Giải thích: Hub là class kế thừa `Hub`, các public method là thứ client gọi được.

Để cấu hình SignalR trong `Program.cs` cần những gì?
A. Chỉ cần `MapHub`
B. `AddSignalR()` đăng ký services; `MapHub<ChatHub>("/chatHub")` ánh xạ hub tới URL endpoint
C. Chỉ cần cài thư viện JavaScript phía client
D. Không cần cấu hình gì
Đáp án: B
Giải thích: `AddSignalR()` + `MapHub<T>(url)`.

`Clients.Others` gọi method trên đối tượng nào?
A. Tất cả client đang kết nối trừ client đã gọi hub method
B. Chỉ client đã gọi hub method
C. Mọi client đang kết nối
D. Chỉ một group
Đáp án: A
Giải thích: `Others` = mọi client trừ caller.

`Context.ConnectionId` trả về gì?
A. Tên người dùng
B. Địa chỉ IP
C. ID duy nhất do SignalR gán cho mỗi connection
D. Tên group
Đáp án: C
Giải thích: `ConnectionId` là ID duy nhất của mỗi connection.

Cách khắc phục nhược điểm "magic string" của `SendAsync("ReceiveMessage", ...)` là gì?
A. Dùng JSON thay cho MessagePack
B. Dùng hub filter
C. Dùng `Context.Items`
D. Strongly typed hub với `Hub<T>` để tránh lỗi runtime do sai tên method
Đáp án: D
Giải thích: `Hub<T>` giúp bắt lỗi sai tên ngay lúc biên dịch thay vì lỗi runtime.

Attribute `[HubMethodName("SendMessageToUser")]` có tác dụng gì?
A. Đổi tên class hub
B. Chỉ định tên mà client dùng để gọi method, thay cho tên method .NET mặc định
C. Ẩn method khỏi client
D. Cấu hình group
Đáp án: B
Giải thích: `[HubMethodName]` đổi tên method mà client gọi.

Trong `OnDisconnectedAsync(Exception exception)`, tham số `exception` là `null` khi nào?
A. Khi client ngắt kết nối có chủ đích (ví dụ gọi `connection.stop()`)
B. Khi có lỗi mạng (network failure)
C. Khi server bị crash
D. Khi connection bị timeout
Đáp án: A
Giải thích: `null` khi ngắt chủ động; có exception khi ngắt do lỗi như mạng.

Phát biểu nào đúng về Hub filters?
A. Chỉ có trước ASP.NET Core 3.1
B. Chỉ chạy phía client
C. Có từ ASP.NET Core 5.0, implement `IHubFilter` với `InvokeMethodAsync`; global filters chạy trước local filters
D. Thay thế hoàn toàn class `Hub`
Đáp án: C
Giải thích: Hub filter từ ASP.NET Core 5.0, implement `IHubFilter`; global chạy trước local.

MessagePack so với JSON trong SignalR thì thế nào?
A. Lớn hơn và chậm hơn JSON
B. Dễ đọc bằng mắt thường hơn
C. Hoàn toàn giống nhau
D. Là binary serialization nhỏ gọn và nhanh hơn (ví dụ slide: JSON 27 byte, MessagePack 18 byte)
Đáp án: D
Giải thích: MessagePack nhỏ và nhanh hơn JSON (18 byte so với 27 byte trong ví dụ).

Trong lab Entity Framework + SignalR, vì sao sau `SaveChangesAsync()` ta gọi `Clients.All.SendAsync("LoadProducts")`?
A. Để lưu dữ liệu vào database
B. Để báo mọi client tải lại danh sách sản phẩm (client gọi `LoadProdData()` rồi vẽ lại bảng), giúp dữ liệu đồng bộ real-time
C. Để tạo migration mới
D. Để xóa cache của server
Đáp án: B
Giải thích: Báo mọi client tải lại danh sách để dữ liệu đồng bộ real-time.
