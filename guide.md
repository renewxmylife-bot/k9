# Hướng Dẫn Tích Hợp Giao Diện HTML/CSS Đè Lên Unity WebGL

Tài liệu này hướng dẫn cách xây dựng các giao diện người dùng (Popups, Shop, Prompts) bằng mã HTML/CSS bên ngoài Canvas của Unity WebGL, được gọi động từ C# thông qua cơ chế JavaScript WebGL Plugin (`.jslib`).

---

## 1. Tổng Quan Cơ Chế Hoạt Động

Thay vì nhúng tĩnh mã HTML vào file `index.html` của template WebGL (dễ bị ghi đè khi rebuild game), chúng ta sẽ sử dụng phương pháp **Dynamic DOM Manipulation** thông qua plugin `.jslib`:
1.  **Unity C#** gọi một hàm ngoại trú (`extern`) thông qua JSLib.
2.  **JavaScript JSLib** sẽ:
    *   Tự động chèn mã CSS trang trí vào thẻ `<head>` nếu chưa có.
    *   Dựng động cấu trúc HTML (overlay, container, button, text,...) bằng JavaScript DOM API.
    *   Thêm (append) giao diện này vào thẻ `<body>` của trình duyệt.
    *   Đặt `z-index` và `position: fixed` để giao diện đè lên Canvas của game.
3.  Khi có sự kiện trên giao diện (ví dụ: click mua hàng, click đóng cửa sổ), JavaScript sẽ gọi ngược lại hàm C# trong Unity để cập nhật dữ liệu.

---

## 2. Các Bước Thực Hiện Chi Tiết

### Bước 1: Expose Unity Instance Ra Toàn Cục (Global)

Để JavaScript có thể gửi dữ liệu ngược về Unity, chúng ta cần lưu trữ instance của Unity vào đối tượng `window` trong file `index.html` của WebGL Template.

Tại file `index.html`, trong đoạn script khởi tạo Unity:
```javascript
createUnityInstance(canvas, config, (progress) => {
  // ...
}).then((unityInstance) => {
  // Lưu instance ra biến toàn cục
  window.unityInstance = unityInstance; 
}).catch((message) => {
  alert(message);
});
```

---

### Bước 2: Viết Plugin JavaScript (`.jslib`)

Tạo một file `.jslib` (ví dụ: `MyPlugin.jslib`) đặt trong thư mục: `Assets/Plugins/WebGL/`.

Nội dung file `.jslib` triển khai logic dựng giao diện động:

```javascript
mergeInto(LibraryManager.library, {

  // Hàm mở Shop từ Unity
  ShowCustomShop: function (currentCredits, backendUrlPtr, objectNamePtr, callbackMethodPtr) {
    // Chuyển đổi con trỏ chuỗi từ Unity C# sang JS String
    var backendUrl = UTF8ToString(backendUrlPtr);
    var objectName = UTF8ToString(objectNamePtr);
    var callbackMethod = UTF8ToString(callbackMethodPtr);

    var modalId = "my-custom-shop-modal";
    
    // 1. Kiểm tra và dọn dẹp modal cũ nếu đã tồn tại
    var existingModal = document.getElementById(modalId);
    if (existingModal) {
      existingModal.remove();
    }

    // 2. Nhúng mã CSS trang trí vào <head>
    var cssId = "my-custom-shop-css";
    var existingCss = document.getElementById(cssId);
    if (!existingCss) {
      var css = document.createElement("style");
      css.id = cssId;
      css.innerHTML = `
        .shop-overlay {
          position: fixed;
          top: 0;
          left: 0;
          width: 100vw;
          height: 100vh;
          background: rgba(0, 0, 0, 0.8);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 99999; /* Đè lên Canvas */
          font-family: sans-serif;
          opacity: 0;
          transition: opacity 0.3s ease;
        }
        .shop-container {
          background: #222;
          color: #fff;
          border-radius: 12px;
          padding: 20px;
          width: 90%;
          max-width: 400px;
          transform: scale(0.9);
          transition: transform 0.3s ease;
        }
        .shop-close-btn {
          float: right;
          cursor: pointer;
          background: none;
          border: none;
          color: red;
          font-size: 20px;
        }
        .shop-buy-btn {
          background: #f59e0b;
          border: none;
          padding: 10px 20px;
          border-radius: 6px;
          cursor: pointer;
          font-weight: bold;
        }
      `;
      document.head.appendChild(css);
    }

    // 3. Dựng cấu trúc HTML động
    var overlay = document.createElement("div");
    overlay.className = "shop-overlay";
    overlay.id = modalId;

    var container = document.createElement("div");
    container.className = "shop-container";
    container.innerHTML = `
      <button class="shop-close-btn" id="shop-close-x">&times;</button>
      <h2>Cửa Hàng Mạng</h2>
      <p>Số mạng hiện tại: <strong>${currentCredits}</strong></p>
      <button class="shop-buy-btn" id="shop-buy-action">Mua Thêm 10 Mạng</button>
    `;

    overlay.appendChild(container);
    document.body.appendChild(overlay);

    // 4. Hiệu ứng Fade-in mượt mà
    setTimeout(function () {
      overlay.style.opacity = "1";
      container.style.transform = "scale(1)";
    }, 10);

    // 5. Lắng nghe sự kiện click đóng Modal
    var closeBtn = document.getElementById("shop-close-x");
    closeBtn.onclick = function () {
      closeModal();
    };

    function closeModal() {
      overlay.style.opacity = "0";
      container.style.transform = "scale(0.9)";
      setTimeout(function () {
        overlay.remove();
      }, 300);
    }

    // 6. Lắng nghe sự kiện click nút Mua Mạng
    var buyBtn = document.getElementById("shop-buy-action");
    buyBtn.onclick = function () {
      buyBtn.disabled = true;
      buyBtn.innerText = "Đang xử lý...";

      // Gửi HTTP Request lên Backend để xử lý giao dịch
      var xhr = new XMLHttpRequest();
      xhr.open("POST", backendUrl + "/api/buy-credits", true);
      xhr.setRequestHeader("Content-Type", "application/json");
      xhr.onload = function () {
        if (xhr.status === 200) {
          alert("Mua mạng thành công!");
          closeModal();

          // Gửi phản hồi về Unity C# để cập nhật lại UI game
          var inst = window.unityInstance || window.gameInstance;
          if (inst) {
            inst.SendMessage(objectName, callbackMethod, "success");
          }
        } else {
          alert("Giao dịch thất bại.");
          buyBtn.disabled = false;
          buyBtn.innerText = "Mua Thêm 10 Mạng";
        }
      };
      xhr.send(JSON.stringify({ amount: 10 }));
    };
  }

});
```

---

### Bước 3: Khai Báo Và Gọi Từ Unity C#

Tạo một script quản lý trong Unity (ví dụ: `ShopManager.cs`). Ta cần khai báo hàm extern của JSLib và cài đặt thêm **Mock Logic** để tránh lỗi khi test chạy trực tiếp trong Editor của Unity.

```csharp
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ShopManager : MonoBehaviour
{
    public string backendUrl = "https://my-backend-api.com";

    // 1. Khai báo liên kết với hàm JS trong JSLib
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ShowCustomShop(int currentCredits, string backendUrl, string objectName, string callbackMethod);
    #else
    // Mock phương thức để chạy trên Editor không bị crash
    private static void ShowCustomShop(int currentCredits, string backendUrl, string objectName, string callbackMethod)
    {
        Debug.Log($"[Mock Editor] Mở Shop. Số mạng: {currentCredits}. Backend: {backendUrl}");
        
        // Mô phỏng phản hồi thành công sau 2 giây
        GameObject go = GameObject.Find(objectName);
        if (go != null)
        {
            go.SendMessage(callbackMethod, "success");
        }
    }
    #endif

    // 2. Hàm kích hoạt mở Shop từ Button trong game
    public void OpenShop()
    {
        int currentCredits = PlayerPrefs.GetInt("Credits", 3);
        
        // Gọi xuống JSLib
        ShowCustomShop(currentCredits, backendUrl, gameObject.name, "OnPurchaseComplete");
    }

    // 3. Hàm callback nhận kết quả trả về từ JavaScript
    public void OnPurchaseComplete(string status)
    {
        if (status == "success")
        {
            Debug.Log("Unity đã nhận: Mua hàng thành công, tiến hành cập nhật UI!");
            // Cập nhật điểm/mạng trong PlayerPrefs và UI của game
            int newCredits = PlayerPrefs.GetInt("Credits", 3) + 10;
            PlayerPrefs.SetInt("Credits", newCredits);
            PlayerPrefs.Save();
        }
    }
}
```

---

## 3. Các Lưu Ý Quan Trọng Khi Thiết Kế (Best Practices)

1.  **Dọn dẹp DOM (Cleanup)**: Hãy luôn đảm bảo hàm đóng modal thực hiện `.remove()` phần tử khỏi `document.body` để tránh rác DOM khi người dùng mở đi mở lại shop nhiều lần.
2.  **Đặt tên class CSS độc nhất**: Do CSS này được nhúng trực tiếp vào trang Web chung, hãy đặt tiền tố riêng cho class (ví dụ: `.mygame-shop-overlay`, `.mygame-shop-container`) để tránh bị đụng độ style với các thư viện ngoài.
3.  **Tương thích Mobile & Webview**:
    *   Sử dụng đơn vị `vw` (viewport width) và `vh` (viewport height) hoặc dùng tỷ lệ `%` để giao diện co giãn tốt trên màn hình điện thoại di động (đặc biệt khi chạy trong Telegram WebApp).
    *   Đặt `z-index` tối thiểu `99999` để chắc chắn vượt lên trên Canvas của Unity.
4.  **Kiểm tra và xử lý null cho `unityInstance`**: JavaScript nên tìm kiếm dự phòng cả `window.unityInstance` lẫn `window.gameInstance` để đảm bảo độ tương thích với các phiên bản Unity khác nhau:
    ```javascript
    var inst = window.unityInstance || window.gameInstance || (typeof unityInstance !== 'undefined' ? unityInstance : null);
    ```
