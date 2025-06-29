document.addEventListener('DOMContentLoaded', function () {
  // --- Sidebar Active State Logic ---
  const sections = document.querySelectorAll('.content-section');
  const navLinks = document.querySelectorAll('.sidebar-link');

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        navLinks.forEach(link => {
          const linkHref = link.getAttribute('href');
          link.classList.remove('active');
          if (linkHref && linkHref.substring(1) === entry.target.id) {
            link.classList.add('active');
          }
        });
      }
    });
  }, { rootMargin: '-30% 0px -70% 0px' });

  sections.forEach(section => {
    observer.observe(section);
  });

  // --- Timeline Chart Logic ---
  const ctx = document.getElementById('timelineChart').getContext('2d');
  const timelineData = {
    labels: [
      '最終簡報準備',
      '全面測試與修復',
      '前端互動優化 (Vue)',
      '按讚功能實作',
      '留言功能實作',
      '後端 API 設計',
      '核心 CRUD (貼文)',
      '使用者系統',
      '資料庫設計/建置',
      '地基與環境設定'
    ],
    datasets: [{
      label: '開發週',
      data: [
        [5, 6], // 第六週
        [5, 6], // 第六週
        [4, 5], // 第五週
        [4, 5], // 第五週
        [3, 4], // 第四週
        [3, 4], // 第四週
        [2, 3], // 第三週
        [1, 2], // 第二週
        [1, 2], // 第二週
        [0, 1]  // 第一週
      ],
      backgroundColor: [
        'rgba(192, 132, 252, 0.7)',
        'rgba(192, 132, 252, 0.7)',
        'rgba(129, 140, 248, 0.7)',
        'rgba(129, 140, 248, 0.7)',
        'rgba(96, 165, 250, 0.7)',
        'rgba(96, 165, 250, 0.7)',
        'rgba(52, 211, 153, 0.7)',
        'rgba(251, 146, 60, 0.7)',
        'rgba(248, 113, 113, 0.7)',
        'rgba(248, 113, 113, 0.7)'
      ],
      borderColor: [
        'rgba(192, 132, 252, 1)',
        'rgba(192, 132, 252, 1)',
        'rgba(129, 140, 248, 1)',
        'rgba(129, 140, 248, 1)',
        'rgba(96, 165, 250, 1)',
        'rgba(96, 165, 250, 1)',
        'rgba(52, 211, 153, 1)',
        'rgba(251, 146, 60, 1)',
        'rgba(248, 113, 113, 1)',
        'rgba(248, 113, 113, 1)'
      ],
      borderWidth: 1,
      barPercentage: 0.6,
      categoryPercentage: 0.8,
      borderSkipped: false,
    }]
  };

  new Chart(ctx, {
    type: 'bar',
    data: timelineData,
    options: {
      indexAxis: 'y',
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              const start = context.raw[0];
              return `任務期間: 第 ${start + 1} 週`;
            }
          }
        }
      },
      scales: {
        x: {
          min: 0,
          max: 6,
          grid: { display: true },
          ticks: {
            stepSize: 1,
            callback: function (value) {
              if (value > 0 && value <= 6) return `第 ${value} 週`;
              if (value === 0) return '開始';
              return null;
            }
          }
        },
        y: { grid: { display: false } }
      }
    }
  });
});

// --- Copy Code Logic ---
function copyCode() {
  const codeElement = document.getElementById('api-code');
  const text = codeElement.innerText;

  if (navigator.clipboard && window.isSecureContext) {
    navigator.clipboard.writeText(text).then(() => {
      alert('API 規格已複製！');
    }, () => {
      fallbackCopy(text);
    });
  } else {
    fallbackCopy(text);
  }
}

function fallbackCopy(text) {
  const textArea = document.createElement('textarea');
  textArea.value = text;
  textArea.style.position = 'fixed';
  textArea.style.left = '-9999px';
  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();
  try {
    const successful = document.execCommand('copy');
    if (successful) alert('API 規格已複製！');
    else alert('複製失敗！');
  } catch (err) {
    alert('複製失敗！');
  }
  document.body.removeChild(textArea);
}