# WebBanHang – ASP.NET Core MVC 8 / Jenkins / Ubuntu VPS

Starter repo phục vụ bài tập **GitHub → Jenkins → Restore → Build → Test → Publish → Deploy → Verify**.

## 1. Chạy trên máy cá nhân (Windows/Linux/macOS có .NET 8 SDK)

```bash
dotnet restore WebBanHang.Tests/WebBanHang.Tests.csproj
dotnet test WebBanHang.Tests/WebBanHang.Tests.csproj -c Release
dotnet run --project WebBanHang/WebBanHang.csproj
```

Mở địa chỉ localhost được in ra bởi `dotnet run` (có thể đặt `--urls http://localhost:5050`).
Dữ liệu SQLite lưu trong `WebBanHang/data/webbanhang.db` (đã gitignore).

## 2. Chuẩn bị Jenkins Docker TRÊN CHÍNH UBUNTU VPS

Phải có **docker CLI trong Jenkins container**, và CLI đó kết nối tới **Docker daemon của Ubuntu host** (không phải Docker-in-Docker tách biệt). Trên Ubuntu:

```bash
docker ps
docker exec <JENKINS_CONTAINER_NAME> sh -lc 'command -v docker && docker version'
docker exec <JENKINS_CONTAINER_NAME> sh -lc 'docker info --format "Docker host: {{.Name}}"'
```

Nếu không qua được, chỉnh Jenkins Docker image và compose để có Docker CLI và quyền truy cập Docker daemon host. Không gỡ/cài lại Jenkins đang hoạt động. Có thể dùng docker socket host với quyền phù hợp, nhưng **đây là quyền rất mạnh tương đương quyền điều khiển host**: không chạy pipeline từ mã không đáng tin cậy; giới hạn người được sửa `Jenkinsfile` và kiểm soát PR. Nếu bạn đang dùng DinD, cần đổi Docker endpoint chạy deployment hoặc cấu hình mạng phù hợp.

## 3. Đưa mã lên GitHub

Tạo một repository rỗng (vd `webbanhang-cicd-demo`), giải nén project này, rồi trong thư mục có `Jenkinsfile`:

```bash
git init
git add .
git commit -m "feat: initial aspnet core mvc demo with CI/CD"
git branch -M main
git remote add origin https://github.com/USERNAME/webbanhang-cicd-demo.git
git push -u origin main
```

Bạn cần đăng nhập GitHub bằng phương thức hợp lệ (Git Credential Manager/PAT hoặc SSH) khi push. Đừng đưa password/token vào repository.

## 4. Tạo Jenkins Job

Jenkins → **New Item** → `WebBanHang-CICD` → **Pipeline**.

- **Definition:** Pipeline script from SCM
- **SCM:** Git
- **Repository URL:** URL repo GitHub của bạn
- **Branches to build:** `*/main`
- **Script Path:** `Jenkinsfile`
- **Build Triggers:** GitHub hook trigger for GITScm polling (hoặc triggers `githubPush()` từ Jenkinsfile với plugin GitHub được cài).
- Lần đầu có thể bấm **Build Now** để xác nhận hoạt động.

Plugin cần có: Pipeline, Git, GitHub. Nếu repository riêng tư, cấu hình credential cho Git checkout.

## 5. GitHub Webhook

GitHub repository → Settings → Webhooks → Add webhook:

- Payload URL: `https://TEN-DOMAIN-JENKINS-CUA-BAN/github-webhook/`
- Content type: `application/json`
- Event: Just the push event
- Active: checked

Quan trọng: dấu `/` cuối endpoint; domain Jenkins phải được truy cập công khai qua HTTPS; GitHub Delivery phải thành công. Khi merge vào `main`, GitHub gửi push event và kích hoạt Jenkins.

## 6. Truy cập ứng dụng demo

Sau khi Build #1 SUCCESS:

```bash
docker ps --filter name=webbanhang-demo
docker logs --tail 50 webbanhang-demo
curl http://127.0.0.1:18080/health
```

Từ máy khác: `http://PUBLIC_VPS_IP:18080` (mở inbound TCP 18080 trên UFW và cloud firewall nếu cần). Port này không tranh chấp cổng 80/443 của Jenkins reverse proxy. Có thể sau đó cấu hình subdomain và Nginx reverse proxy nếu muốn HTTPS.

## 7. Demo 3 tình huống theo đề

1. **Thành công:** Tạo nhánh `feature/home-v2`, sửa tiêu đề trong `WebBanHang/Views/Home/Index.cshtml` hoặc CSS, push → Pull Request → merge `main`. Jenkins tự chạy; refresh website thấy nội dung mới và build version tăng.
2. **Build lỗi:** Trên nhánh test, cố ý chèn `this is invalid csharp` vào `Services/OrderCalculator.cs`, chạy pipeline cho nhánh test (job thử riêng hoặc tạm cấu hình branch của job). Build FAIL; container phiên bản main vẫn chạy. **Không merge lỗi vào main.**
3. **Unit Test lỗi:** Trên nhánh test, đổi `return price * quantity;` thành `return price + quantity;`, chạy pipeline cho nhánh test. Build PASS, Test FAIL; stage Deploy không chạy. **Không merge lỗi vào main.**

Để demo an toàn, tạo job Jenkins thứ hai **`WebBanHang-Validation`** với **Definition: Pipeline script from SCM**, **Script Path: `Jenkinsfile.validate`**, và **Branches to build** lần lượt là `*/ci-fail-build` hoặc `*/ci-fail-test`. Job này chỉ chạy Checkout → Restore → Build → Test, **không bao giờ deploy**. Đổi branch của job validation trước mỗi tình huống rồi bấm **Build Now**; job `WebBanHang-CICD` vẫn giữ `*/main`.

## Lưu ý để demo

- SQLite nằm ở named volume `webbanhang-demo-data` nên không mất khi Jenkins thay container; **đừng** dùng `docker volume rm`/`docker compose down -v` với volume này.
- Pipeline có rollback mức demo nếu ứng dụng mới chạy nhưng không qua Verify; không phải zero-downtime deployment.
- Không dùng cấu hình này làm production public lâu dài: demo chưa có đăng nhập/RBAC và dùng EF `EnsureCreated()` thay cho migrations; cần thêm HTTPS và hạn chế truy cập.
- Nếu Jenkins dùng Docker socket host, thao tác `docker rm -f webbanhang-demo` chỉ tác động container demo đúng tên; không tác động container Jenkins.
- `.NET 8` trong Docker SDK và ASP.NET runtime; **không cần cài `dotnet` trên Ubuntu host hay bên trong Jenkins container**.
