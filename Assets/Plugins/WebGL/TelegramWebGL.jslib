mergeInto(LibraryManager.library, {
    GetTelegramInitData: function() {
        if (typeof window.Telegram !== 'undefined' && window.Telegram.WebApp) {
            var user = window.Telegram.WebApp.initDataUnsafe.user;
            if (user) {
                var jsonStr = JSON.stringify({
                    id: user.id,
                    username: user.username || user.first_name || "Player"
                });
                var bufferSize = lengthBytesUTF8(jsonStr) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(jsonStr, buffer, bufferSize);
                return buffer;
            }
        }
        return null;
    },
    ShowShopOverlay: function(shopTypeStr, backendUrlStr, telegramIdStr, usernameStr) {
        var shopType = UTF8ToString(shopTypeStr);
        var backendUrl = UTF8ToString(backendUrlStr);
        var telegramId = UTF8ToString(telegramIdStr);
        var username = UTF8ToString(usernameStr);

        var modalId = "telegram-shop-modal";
        
        // 1. Remove old modal if it exists
        var existingModal = document.getElementById(modalId);
        if (existingModal) {
            existingModal.remove();
        }

        // 2. Inject CSS Style sheet dynamically
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
                    z-index: 999999;
                    font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                    opacity: 0;
                    transition: opacity 0.3s ease;
                    color: #fff;
                }
                .tg-container {
                    background: rgba(255, 255, 255, 0.1);
                    border: 1px solid rgba(255, 255, 255, 0.2);
                    border-radius: 20px;
                    padding: 25px;
                    width: 85%;
                    max-width: 400px;
                    box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
                    text-align: center;
                    transform: scale(0.9);
                    transition: transform 0.3s ease;
                }
                .tg-title {
                    font-size: 24px;
                    font-weight: 700;
                    margin-bottom: 10px;
                    text-shadow: 0 2px 4px rgba(0,0,0,0.5);
                }
                .tg-subtitle {
                    font-size: 14px;
                    color: #ddd;
                    margin-bottom: 20px;
                }
                .tg-list {
                    display: flex;
                    flex-direction: column;
                    gap: 12px;
                    max-height: 320px;
                    overflow-y: auto;
                    padding-right: 5px;
                }
                .tg-card {
                    background: rgba(255, 255, 255, 0.08);
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    border-radius: 12px;
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    padding: 12px 16px;
                    cursor: pointer;
                    transition: transform 0.2s, background 0.2s;
                }
                .tg-card:hover {
                    background: rgba(255, 255, 255, 0.15);
                    transform: translateY(-2px);
                }
                .tg-details {
                    text-align: left;
                }
                .tg-name {
                    font-size: 16px;
                    font-weight: 600;
                }
                .tg-desc {
                    font-size: 12px;
                    color: #bbb;
                }
                .tg-price {
                    background: #f1c40f;
                    color: #000;
                    font-weight: 700;
                    padding: 6px 12px;
                    border-radius: 8px;
                    font-size: 14px;
                    white-space: nowrap;
                }
                .tg-close-btn {
                    background: rgba(255, 255, 255, 0.2);
                    border: none;
                    color: white;
                    border-radius: 8px;
                    padding: 8px 16px;
                    font-size: 14px;
                    cursor: pointer;
                    margin-top: 20px;
                    transition: background 0.2s;
                }
                .tg-close-btn:hover {
                    background: rgba(255, 255, 255, 0.3);
                }
            `;
            document.head.appendChild(css);
        }

        // 3. Define store items
        var livesBundles = [
            { id: "10", name: "10 Lives", desc: "Small continuation bundle", price: 100 },
            { id: "20", name: "20 Lives", desc: "Standard continuation bundle", price: 200 },
            { id: "30", name: "30 Lives", desc: "Best value starter pack", price: 300 },
            { id: "60", name: "60 Lives", desc: "Pro player package", price: 500 },
            { id: "140", name: "140 Lives", desc: "Infinite challenge bundle", price: 1000 }
        ];

        var skinItems = [
            { id: "red", name: "Red Aura", desc: "A fierce red fiery aura", price: 5000 },
            { id: "yellow", name: "Yellow Aura", desc: "A golden glowing aura", price: 5000 },
            { id: "green", name: "Green Aura", desc: "A natural glowing aura", price: 5000 },
            { id: "blue", name: "Blue Aura", desc: "A peaceful blue sky aura", price: 5000 },
            { id: "orange", name: "Orange Aura", desc: "A glowing sunset aura", price: 5000 }
        ];

        var premiumItem = { id: "premium", name: "Premium Upgrade", desc: "Disable advertisements forever!", price: 10000 };

        var items = [];
        var titleText = "Shop";
        if (shopType === "lives") {
            titleText = "Lives Store";
            items = livesBundles;
        } else if (shopType === "skins") {
            titleText = "Aura Skins Store";
            items = skinItems;
        } else if (shopType === "premium") {
            titleText = "Premium Upgrade";
            items = [premiumItem];
        }

        // 4. Create DOM elements dynamically
        var overlay = document.createElement("div");
        overlay.className = "tg-overlay";
        overlay.id = modalId;

        var container = document.createElement("div");
        container.className = "tg-container";

        var title = document.createElement("div");
        title.className = "tg-title";
        title.innerText = titleText;

        var subtitle = document.createElement("div");
        subtitle.className = "tg-subtitle";
        subtitle.innerText = "Enhance your game with Telegram Stars!";

        var list = document.createElement("div");
        list.className = "tg-list";

        items.forEach(function(item) {
            var card = document.createElement("div");
            card.className = "tg-card";
            card.onclick = function() {
                purchaseItem(item.id);
            };

            card.innerHTML = `
                <div class="tg-details">
                    <div class="tg-name">${item.name}</div>
                    <div class="tg-desc">${item.desc}</div>
                </div>
                <div class="tg-price">
                    ★ ${item.price}
                </div>
            `;
            list.appendChild(card);
        });

        var closeBtn = document.createElement("button");
        closeBtn.className = "tg-close-btn";
        closeBtn.innerText = "Back to Game";
        closeBtn.onclick = function() {
            closeModal();
        };

        container.appendChild(title);
        container.appendChild(subtitle);
        container.appendChild(list);
        container.appendChild(closeBtn);
        overlay.appendChild(container);
        document.body.appendChild(overlay);

        // 5. Fade-in animation
        setTimeout(function() {
            overlay.style.opacity = "1";
            container.style.transform = "scale(1)";
        }, 10);

        function closeModal() {
            overlay.style.opacity = "0";
            container.style.transform = "scale(0.9)";
            setTimeout(function() {
                overlay.remove();
            }, 300);
        }

        function purchaseItem(itemId) {
            fetch(backendUrl + "/api/create-invoice", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    telegram_id: telegramId,
                    item_type: shopType,
                    item_id: itemId
                })
            })
            .then(function(res) { return res.json(); })
            .then(function(data) {
                if (data.success && data.invoiceLink) {
                    if (typeof Telegram !== 'undefined' && Telegram.WebApp && Telegram.WebApp.openInvoice) {
                        Telegram.WebApp.openInvoice(data.invoiceLink, function(status) {
                            if (status === "paid") {
                                console.log("[INVOICE] Payment successful!");
                                syncUserData();
                            } else {
                                console.log("[INVOICE] Payment failed or cancelled. Status: " + status);
                            }
                        });
                    } else {
                        console.log("[INVOICE EDITOR] Simulating successful payment for testing...");
                        alert("Payment simulated successfully!");
                        syncUserData();
                    }
                } else {
                    alert("Error creating invoice: " + (data.error || "Unknown error"));
                }
            })
            .catch(function(err) {
                console.error("Failed to create invoice:", err);
                alert("Network error. Could not contact backend.");
            });
        }

        function syncUserData() {
            fetch(backendUrl + "/api/user/" + telegramId + "?username=" + encodeURIComponent(username))
            .then(function(res) { return res.json(); })
            .then(function(userData) {
                var inst = window.unityInstance || window.gameInstance || (typeof unityInstance !== 'undefined' ? unityInstance : null);
                if (inst) {
                    inst.SendMessage("TelegramManager", "OnPurchaseSuccess", JSON.stringify(userData));
                }
                closeModal();
            });
        }
    },
    ConsoleLogAd: function(msgStr) {
        var msg = UTF8ToString(msgStr);
        console.log("Play Ad: " + msg);
    }
});
