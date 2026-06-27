# Hướng Dẫn Tích Hợp Telegram SDK & Tối Ưu Tỷ Lệ Giao Diện (9:16) Trong Unity WebGL

Tài liệu này tổng hợp 3 giải pháp tối ưu đã được áp dụng thành công trong dự án để giúp bạn tái sử dụng cho các dự án game Telegram Mini App / WebGL khác.

---

## 1. Nhúng Telegram WebApp SDK
Để WebGL của bạn giao tiếp được với các tính năng của Telegram (Thanh toán Stars, thông tin User, rung thiết bị, mở popups...), bạn cần nhúng SDK của Telegram vào thẻ `<head>` của file `index.html`.

### Cách chèn vào file `index.html`:
```html
<head>
  <!-- Nhúng SDK Telegram WebApp -->
  <script src="https://telegram.org/js/telegram-web-app.js"></script>
</head>
```

### Kích hoạt trong Javascript khởi tạo Unity:
```javascript
createUnityInstance(canvas, config, (progress) => {
  // ...
}).then((unityInstance) => {
  // 1. Lưu instance vào window để JSLib truy cập gửi dữ liệu về Unity C#
  window.unityInstance = unityInstance;

  // 2. Báo hiệu Telegram WebApp đã sẵn sàng và mở rộng toàn màn hình
  if (typeof Telegram !== 'undefined' && Telegram.WebApp) {
    Telegram.WebApp.ready();
    Telegram.WebApp.expand();
  }
});
```

---

## 2. CSS Tối Ưu Tỷ Lệ Màn Hình Dọc (9:16) & Chống Phình/Cuộn Màn Hình
Khi chơi game dọc trên màn hình máy tính (PC/Laptop), giao diện game dễ bị phình to ngang hoặc xuất hiện thanh cuộn lên cuộn xuống rất khó chịu. Đoạn CSS dưới đây giúp cố định tỉ lệ **9:16** ở trung tâm màn hình PC và hiển thị tràn viền trên Mobile.

### Chèn đoạn mã CSS này vào phần `<style>` của file `index.html`:
```css
html, body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  background-color: #000;
  overflow: hidden; /* Chặn hoàn toàn hành vi cuộn trang (scroll) */
  display: flex;
  justify-content: center;
  align-items: center;
  user-select: none;
  -webkit-user-select: none;
}

#unity-container {
  width: 100%;
  height: 100%;
  position: relative;
}

/* Tự động khóa tỉ lệ dọc 9:16 trên màn hình PC rộng */
@media (min-aspect-ratio: 9/16) {
  #unity-container {
    width: calc(100vh * 9 / 16); /* Chiều rộng = 9/16 chiều cao màn hình */
    height: 100vh;
    box-shadow: 0 0 30px rgba(0, 0, 0, 0.8); /* Tạo bóng đổ sang trọng */
  }
}

#unity-canvas {
  width: 100%;
  height: 100%;
  display: block;
}
```

---

## 3. Tạo Giao Diện HTML/CSS Đè Lên Canvas Động (Dynamic DOM Manipulation via JSLib)
Thay vì sửa chay hoặc nhúng cứng HTML vào template WebGL (rất dễ lỗi màn hình đen hoặc bị ghi đè khi rebuild từ Unity), chúng ta sẽ sử dụng plugin JSLib (`Assets/Plugins/WebGL/TelegramWebGL.jslib`) để sinh giao diện HTML/CSS động từ code C#.

### Cách viết hàm JSLib dựng giao diện động:
```javascript
mergeInto(LibraryManager.library, {
  ShowShopOverlay: function (shopTypeStr, backendUrlStr, telegramIdStr, usernameStr) {
    // Chuyển đổi con trỏ chuỗi C# sang JS String
    var shopType = UTF8ToString(shopTypeStr);
    var backendUrl = UTF8ToString(backendUrlStr);
    var telegramId = UTF8ToString(telegramIdStr);
    var username = UTF8ToString(usernameStr);

    var modalId = "telegram-shop-modal";
    
    // 1. Dọn dẹp modal cũ (nếu có) để tránh rác DOM
    var existingModal = document.getElementById(modalId);
    if (existingModal) {
        existingModal.remove();
    }

    // 2. Nhúng CSS Style cho Shop đè lên Canvas
    var cssId = "telegram-shop-css";
    var existingCss = document.getElementById(cssId);
    if (!existingCss) {
        var css = document.createElement("style");
        css.id = cssId;
        css.innerHTML = `
            .tg-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                background: rgba(0, 0, 0, 0.7);
                backdrop-filter: blur(10px);
                -webkit-backdrop-filter: blur(10px);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 999999; /* Luôn lớn hơn Canvas của Unity */
                font-family: sans-serif;
                opacity: 0;
                transition: opacity 0.3s ease;
            }
            .tg-container {
                background: rgba(255, 255, 255, 0.1);
                border: 1px solid rgba(255, 255, 255, 0.2);
                border-radius: 20px;
                padding: 25px;
                width: 85%;
                max-width: 400px;
                transform: scale(0.9);
                transition: transform 0.3s ease;
            }
        `;
        document.head.appendChild(css);
    }

    // 3. Dựng cấu trúc HTML động bằng JavaScript DOM API
    var overlay = document.createElement("div");
    overlay.className = "tg-overlay";
    overlay.id = modalId;

    var container = document.createElement("div");
    container.className = "tg-container";
    container.innerHTML = `
        <h2 style="color: #fff;">Cửa hàng</h2>
        <p style="color: #ddd;">Chào mừng ${username}!</p>
        <button id="tg-close-btn" style="padding: 10px 20px; border-radius: 8px; cursor: pointer;">Đóng</button>
    `;

    overlay.appendChild(container);
    document.body.appendChild(overlay);

    // 4. Hiệu ứng Fade-in mượt mà
    setTimeout(function() {
        overlay.style.opacity = "1";
        container.style.transform = "scale(1)";
    }, 10);

    // 5. Gắn sự kiện click đóng cửa sổ
    var closeBtn = document.getElementById("tg-close-btn");
    closeBtn.onclick = function() {
        overlay.style.opacity = "0";
        container.style.transform = "scale(0.9)";
        setTimeout(function() {
            overlay.remove();
        }, 300);
    };

    // 6. Gửi dữ liệu ngược về Unity C# sau khi xử lý thành công
    // Ví dụ: gọi API và đồng bộ
    /*
    var inst = window.unityInstance || window.gameInstance;
    if (inst) {
        inst.SendMessage("TênGameObjectC#", "TênHàmC#", "dữ liệu chuỗi");
    }
    */
  }
});
```

---

## 4. Tóm Tắt Quy Trình Tạo WebGL Template Mới Trực Tiếp Trong Dự Án Unity
Để đóng gói sẵn `index.html` và `css` trên thành một Template tái sử dụng trực tiếp trong Unity:
1. Tạo thư mục: `Assets/WebGLTemplates/<TênTemplate>` (Ví dụ: `Assets/WebGLTemplates/Telegram`).
2. Tạo file cấu hình `template.json`:
   ```json
   {
     "name": "Telegram",
     "thumbnail": "thumbnail.png",
     "description": "Template tối ưu màn hình dọc 9:16 và tích hợp sẵn Telegram SDK.",
     "width": 1080,
     "height": 1920
   }
   ```
3. Tạo file `index.html` sạch, đặt các biến của Unity như `{{{ DATA_FILENAME }}}`, `{{{ WASM_FILENAME }}}` trực tiếp và chèn đoạn code HTML/CSS cấu hình dọc ở trên vào.
4. Khi build game, bạn chỉ cần chọn template **Telegram** trong **Player Settings -> Resolution and Presentation**.
