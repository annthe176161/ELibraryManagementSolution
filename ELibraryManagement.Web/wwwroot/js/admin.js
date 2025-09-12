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
});

function initializeAdminDashboard() {
  // Add loading animation to cards on page load
  $(".admin-card").hide().fadeIn(500);

  // Animate statistics cards
  $(".stat-card").each(function (index) {
    $(this)
      .delay(index * 100)
      .fadeIn(300);
  });

  // Smooth scrolling for sidebar links
  $(".sidebar .nav-link").on("click", function (e) {
    // Add loading effect
    if (!$(this).hasClass("active")) {
      $(this).append('<span class="loading-spinner ms-2"></span>');
    }
  });
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
  showNotification("Có lỗi xảy ra. Vui lòng thử lại.", "error");
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
