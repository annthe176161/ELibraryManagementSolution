// Admin Dashboard JavaScript

$(document).ready(function () {
  // Initialize admin dashboard
  initializeAdminDashboard();

  // Handle sidebar toggle for mobile
  initializeSidebarToggle();

  // Initialize tooltips
  initializeTooltips();

  // Handle responsive behavior
  handleResponsiveLayout();

  // Initialize enhanced features
  initializeEnhancedFeatures();
});

function initializeAdminDashboard() {
  // Add enhanced loading animation to cards on page load
  $(
    ".enhanced-stat-card, .quick-access-card, .analytics-card, .activity-card, .quick-actions-card"
  )
    .hide()
    .each(function (index) {
      $(this)
        .delay(index * 150)
        .fadeIn(500)
        .addClass("fade-in");
    });

  // Animate statistics cards with count-up effect
  animateStatNumbers();

  // Smooth scrolling for sidebar links
  $(".sidebar .nav-link").on("click", function (e) {
    // Add loading effect
    if (!$(this).hasClass("active")) {
      $(this).append('<span class="loading-spinner ms-2"></span>');
    }
  });
}

// Count-up animation for stat numbers
function animateStatNumbers() {
  try {
    $(".stat-number[data-count]").each(function () {
      const $this = $(this);
      const countTo = parseInt($this.attr("data-count")) || 0;

      if (isNaN(countTo)) {
        console.warn("Invalid count value for element:", this);
        $this.text("0");
        return;
      }

      $({ countNum: 0 }).animate(
        {
          countNum: countTo,
        },
        {
          duration: 2000,
          easing: "swing",
          step: function () {
            $this.text(Math.floor(this.countNum));
          },
          complete: function () {
            $this.text(countTo);
          },
        }
      );
    });
  } catch (error) {
    console.error("Error in animateStatNumbers:", error);
  }
}

function initializeEnhancedFeatures() {
  // Chart controls functionality
  $(".chart-controls .btn").on("click", function () {
    $(".chart-controls .btn").removeClass("active");
    $(this).addClass("active");

    // Here you would update the chart data based on selected period
    const period = $(this).data("period");
    updateActivityChart(period);
  });

  // Activity refresh
  window.refreshActivity = function () {
    const $btn = $('[onclick="refreshActivity()"]');
    const originalHtml = $btn.html();

    $btn.html('<i class="fas fa-spinner fa-spin"></i>').prop("disabled", true);

    // Simulate refresh
    setTimeout(() => {
      $btn.html(originalHtml).prop("disabled", false);
      showNotification("Hoạt động đã được cập nhật", "success");
    }, 1500);
  };

  // Dashboard refresh
  window.refreshDashboard = function () {
    const $btn = $('[onclick="refreshDashboard()"]');
    const originalHtml = $btn.html();

    $btn.html('<i class="fas fa-spinner fa-spin"></i>').prop("disabled", true);

    // Simulate refresh
    setTimeout(() => {
      $btn.html(originalHtml).prop("disabled", false);
      animateStatNumbers();
      showNotification("Dashboard đã được cập nhật", "success");
    }, 2000);
  };

  // Quick action functions
  window.addNewBook = function () {
    window.location.href = "/Admin/Books";
  };

  window.processReturns = function () {
    window.location.href = "/Admin/Borrows";
  };

  window.viewOverdueBooks = function () {
    window.location.href = "/Admin/Borrows?filter=overdue";
  };

  window.generateReport = function () {
    showNotification("Đang tạo báo cáo...", "info");
    // Implement report generation
  };

  window.systemBackup = function () {
    showConfirmDialog(
      "Xác nhận sao lưu",
      "Bạn có chắc chắn muốn thực hiện sao lưu dữ liệu?",
      function () {
        showNotification("Đang thực hiện sao lưu...", "info");
        // Implement backup functionality
      }
    );
  };

  window.systemSettings = function () {
    window.location.href = "/Admin/Settings";
  };

  // Initialize activity chart
  initializeActivityChart();
}

function initializeActivityChart() {
  const ctx = document.getElementById("activityChart");
  if (!ctx) return;

  // Sample data - replace with real data from your API
  const chartData = {
    labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
    datasets: [
      {
        label: "Mượn sách",
        data: [12, 19, 3, 5, 2, 3, 9],
        borderColor: "#667eea",
        backgroundColor: "rgba(102, 126, 234, 0.1)",
        tension: 0.4,
      },
      {
        label: "Trả sách",
        data: [8, 15, 7, 12, 8, 6, 10],
        borderColor: "#56ab2f",
        backgroundColor: "rgba(86, 171, 47, 0.1)",
        tension: 0.4,
      },
    ],
  };

  new Chart(ctx, {
    type: "line",
    data: chartData,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "top",
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: {
            color: "rgba(0,0,0,0.05)",
          },
        },
        x: {
          grid: {
            color: "rgba(0,0,0,0.05)",
          },
        },
      },
    },
  });
}

function updateActivityChart(period) {
  // Update chart based on selected period
  // This would typically fetch new data from your API
  showNotification(`Đã cập nhật biểu đồ cho ${period} ngày`, "info");
}

function initializeSidebarToggle() {
  // Create toggle button for mobile
  if (!$(".sidebar-toggle").length) {
    $(
      '<button class="btn sidebar-toggle d-lg-none"><i class="fas fa-bars"></i></button>'
    ).insertAfter(".admin-navbar");
  }

  // Handle sidebar toggle
  $(".navbar-toggler, .sidebar-toggle").on("click", function () {
    $("#sidebar").toggleClass("show");

    // Add overlay for mobile
    if ($("#sidebar").hasClass("show")) {
      if (!$(".sidebar-overlay").length) {
        $('<div class="sidebar-overlay"></div>')
          .appendTo("body")
          .on("click", function () {
            $("#sidebar").removeClass("show");
            $(this).remove();
          });
      }
    } else {
      $(".sidebar-overlay").remove();
    }
  });

  // Auto-hide sidebar on mobile when clicking links
  $(".sidebar .nav-link").on("click", function () {
    if ($(window).width() < 992) {
      setTimeout(function () {
        $("#sidebar").removeClass("show");
        $(".sidebar-overlay").remove();
      }, 300);
    }
  });
}

function initializeTooltips() {
  // Enable Bootstrap tooltips
  var tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
  );
  var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });
}

function handleResponsiveLayout() {
  // Handle window resize
  $(window).on("resize", function () {
    if ($(window).width() >= 992) {
      $("#sidebar").removeClass("show");
      $(".sidebar-overlay").remove();
    }
  });

  // Adjust table responsiveness
  $(".admin-table .table").each(function () {
    if (!$(this).parent().hasClass("table-responsive")) {
      $(this).wrap('<div class="table-responsive"></div>');
    }
  });
}

// Utility Functions
function showNotification(message, type = "success") {
  const alertClass =
    type === "success"
      ? "alert-success"
      : type === "error"
      ? "alert-danger"
      : type === "warning"
      ? "alert-warning"
      : "alert-info";

  const notification = $(`
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
             style="top: 80px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-${
              type === "success"
                ? "check-circle"
                : type === "error"
                ? "exclamation-circle"
                : type === "warning"
                ? "exclamation-triangle"
                : "info-circle"
            } me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);

  $("body").append(notification);

  // Auto-hide after 5 seconds
  setTimeout(function () {
    notification.alert("close");
  }, 5000);
}

function showConfirmDialog(title, message, onConfirm) {
  const modal = $(`
        <div class="modal fade" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p>${message}</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                        <button type="button" class="btn btn-danger confirm-btn">Xác nhận</button>
                    </div>
                </div>
            </div>
        </div>
    `);

  modal.find(".confirm-btn").on("click", function () {
    onConfirm();
    modal.modal("hide");
  });

  modal.on("hidden.bs.modal", function () {
    modal.remove();
  });

  $("body").append(modal);
  modal.modal("show");
}

// Table utilities
function initializeDataTable(tableSelector, options = {}) {
  const defaultOptions = {
    pageLength: 25,
    responsive: true,
    language: {
      url: "//cdn.datatables.net/plug-ins/1.13.7/i18n/vi.json",
    },
  };

  const finalOptions = { ...defaultOptions, ...options };

  if ($.fn.DataTable) {
    $(tableSelector).DataTable(finalOptions);
  }
}

// Search functionality
function initializeTableSearch(tableSelector, searchInputSelector) {
  $(searchInputSelector).on("keyup", function () {
    const value = $(this).val().toLowerCase();
    $(tableSelector + " tbody tr").filter(function () {
      $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
    });
  });
}

// Chart utilities (for future dashboard charts)
function createChart(canvasId, type, data, options = {}) {
  if (typeof Chart !== "undefined") {
    const ctx = document.getElementById(canvasId).getContext("2d");
    return new Chart(ctx, {
      type: type,
      data: data,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        ...options,
      },
    });
  }
}

// Export functionality
function exportTableToCSV(tableSelector, filename = "export.csv") {
  const table = $(tableSelector);
  const rows = [];

  // Get headers
  const headers = [];
  table.find("thead th").each(function () {
    headers.push($(this).text().trim());
  });
  rows.push(headers.join(","));

  // Get data rows
  table.find("tbody tr:visible").each(function () {
    const row = [];
    $(this)
      .find("td")
      .each(function () {
        row.push('"' + $(this).text().trim().replace(/"/g, '""') + '"');
      });
    rows.push(row.join(","));
  });

  // Create and download file
  const csvContent = rows.join("\n");
  const blob = new Blob(["\ufeff" + csvContent], {
    type: "text/csv;charset=utf-8;",
  });
  const link = document.createElement("a");

  if (link.download !== undefined) {
    const url = URL.createObjectURL(blob);
    link.setAttribute("href", url);
    link.setAttribute("download", filename);
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}

// Global error handler
window.addEventListener("error", function (e) {
  console.error("Admin Dashboard Error:", e.error);

  // Only show toast for critical errors, not all JavaScript errors
  if (
    e.error &&
    e.error.name &&
    (e.error.name === "NetworkError" || e.error.name === "TypeError")
  ) {
    // Only show error notification for network or critical type errors
    showNotification("Có lỗi xảy ra. Vui lòng thử lại.", "error");
  }
});

// Add sidebar overlay styles
$("<style>")
  .prop("type", "text/css")
  .html(
    `
        .sidebar-overlay {
            position: fixed;
            top: 60px;
            left: 0;
            width: 100%;
            height: calc(100vh - 60px);
            background: rgba(0,0,0,0.5);
            z-index: 1019;
            opacity: 0;
            animation: fadeIn 0.3s forwards;
        }
        
        @keyframes fadeIn {
            to { opacity: 1; }
        }
        
        @media (min-width: 992px) {
            .sidebar-overlay {
                display: none !important;
            }
        }
    `
  )
  .appendTo("head");
