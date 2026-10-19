// TN Tea - Client-side scripts

// Hiển thị thông báo Toast / Alert tự biến mất sau 5s
window.showSiteToast = function (message, type = 'success') {
    let placeholder = document.getElementById('liveAlertPlaceholder');
    if (!placeholder) {
        placeholder = document.createElement('div');
        placeholder.id = 'liveAlertPlaceholder';
        placeholder.className = 'container my-2';
        const main = document.querySelector('main');
        if (main) main.insertBefore(placeholder, main.firstChild);
        else document.body.appendChild(placeholder);
    }

    const alertEl = document.createElement('div');
    const isSuccess = type === 'success';
    alertEl.className = `alert ${isSuccess ? 'alert-success' : 'alert-danger'} alert-dismissible fade show rounded-4 shadow-sm border-0 mb-3`;
    alertEl.style.backgroundColor = isSuccess ? '#E2F7E9' : '#FDE8EA';
    alertEl.style.color = isSuccess ? '#1B5E20' : '#B71C1C';
    alertEl.role = 'alert';
    alertEl.innerHTML = `
        <i class="bi ${isSuccess ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'} me-2"></i>
        <span>${message}</span>
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    placeholder.appendChild(alertEl);

    // Tự động mờ dần và biến mất sau đúng 5 giây (5s)
    setTimeout(function () {
        alertEl.style.transition = 'opacity 0.6s ease, max-height 0.6s ease, margin 0.6s ease, padding 0.6s ease';
        alertEl.style.opacity = '0';
        setTimeout(function () {
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
                bsAlert.close();
            } else {
                alertEl.remove();
            }
        }, 600);
    }, 5000);
};

// Cập nhật số lượng giỏ hàng trên thanh điều hướng và tạo hiệu ứng nhún
window.updateCartBadge = function (count) {
    const badge = document.getElementById('globalCartBadge');
    const cartBtn = document.getElementById('globalCartBtn');

    if (badge) {
        badge.textContent = count;
        if (count > 0) {
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    }

    if (cartBtn) {
        cartBtn.classList.remove('cart-bounce');
        // Force reflow để trigger lại animation
        void cartBtn.offsetWidth;
        cartBtn.classList.add('cart-bounce');
    }
};

document.addEventListener("DOMContentLoaded", function () {
    // Tự động đóng các thông báo flash hiện có sau 5 giây
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alertEl) {
        setTimeout(function () {
            alertEl.style.transition = 'opacity 0.5s ease, margin 0.5s ease, padding 0.5s ease';
            alertEl.style.opacity = '0';
            setTimeout(function () {
                if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                    const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
                    bsAlert.close();
                } else {
                    alertEl.remove();
                }
            }, 500);
        }, 5000);
    });

    // Bắt sự kiện click nút "Cho vào giỏ hàng" (ở trang Menu và Danh sách món)
    document.addEventListener("click", function (e) {
        const btn = e.target.closest('.btn-add-cart-ajax');
        if (!btn) return;

        e.preventDefault();
        const url = btn.getAttribute('href');
        const originalHtml = btn.innerHTML;

        // Hiệu ứng đang thêm món
        btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Đang thêm...`;
        btn.style.pointerEvents = 'none';

        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'application/json'
            }
        })
        .then(response => {
            if (!response.ok) throw new Error('Network error');
            return response.json();
        })
        .then(data => {
            if (data && data.success) {
                // Đổi nút thành Đã thêm
                btn.innerHTML = `<i class="bi bi-check2-circle me-1"></i> Đã thêm!`;
                
                // Cập nhật số lượng giỏ hàng trên Header & rung icon giỏ
                if (typeof data.cartCount !== 'undefined') {
                    window.updateCartBadge(data.cartCount);
                }

                // Hiển thị thông báo 5s ở đầu trang
                window.showSiteToast(data.message || 'Đã thêm vào giỏ hàng!', 'success');

                setTimeout(() => {
                    btn.innerHTML = originalHtml;
                    btn.style.pointerEvents = '';
                }, 1200);
            } else {
                window.location.href = url; // Fallback
            }
        })
        .catch(err => {
            console.warn('AJAX add to cart failed, fallback to normal navigation:', err);
            window.location.href = url;
        });
    });

    // Bắt sự kiện submit form thêm vào giỏ hàng (ở trang Chi tiết món Details)
    document.addEventListener("submit", function (e) {
        const form = e.target.closest('.form-add-cart-ajax');
        if (!form) return;

        e.preventDefault();
        const submitBtn = form.querySelector('button[type="submit"]');
        const originalHtml = submitBtn ? submitBtn.innerHTML : '';
        if (submitBtn) {
            submitBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-1" role="status"></span> Đang thêm...`;
            submitBtn.disabled = true;
        }

        const formData = new FormData(form);

        fetch(form.action, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'application/json'
            }
        })
        .then(response => {
            if (!response.ok) throw new Error('Network error');
            return response.json();
        })
        .then(data => {
            if (data && data.success) {
                if (submitBtn) {
                    submitBtn.innerHTML = `<i class="bi bi-check2-circle me-1"></i> Đã thêm!`;
                }

                if (typeof data.cartCount !== 'undefined') {
                    window.updateCartBadge(data.cartCount);
                }

                window.showSiteToast(data.message || 'Đã thêm vào giỏ hàng!', 'success');

                setTimeout(() => {
                    if (submitBtn) {
                        submitBtn.innerHTML = originalHtml;
                        submitBtn.disabled = false;
                    }
                }, 1200);
            } else {
                form.submit(); // Fallback
            }
        })
        .catch(err => {
            console.warn('AJAX form add to cart failed, fallback:', err);
            form.submit();
        });
    });
});
