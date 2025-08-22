using Microsoft.AspNetCore.Mvc;

namespace Matrix.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            var members = new[]
            {
                new {
                    index = 0,
                    twName = "陳昱宏",
                    engName = "Eddy",
                    email = "eddychen101020@gmail.com",
                    github = "eddychen86",
                    img = "eddy.png",
                    level = "組長",
                    description = "擁有 2~3 年前端開發經歷的工程師，正為全端工程師為目標邁進。",
                    skill = new string[] {
                        "SCSS", "JS", "Tailwindcss", "Vue", "ASP.NET Core", "SQL"
                    },
                    works = new {
                        envs = new string[] {
                            "專案主題構想",
                            "資料庫架構構思",
                            "UI 設計",
                            "建置專案環境",
                            "設定多國語系",
                            "協助衝突除錯",
                            "提供前端技術支援",
                            "負責資料庫的建置與管理",
                            "SMTP 設置",
                            "專案打包與部屬",
                        },
                        pages = new string[] {
                            "「登入」頁面製作",
                            "「註冊」頁面製作",
                            "「關於我們」頁面製作",
                            "「後臺儀錶板」頁面製作",
                            "「網站設置」頁面製作",
                            "「免責聲明」頁面製作",
                        }
                    }
                },
                new {
                    index = 1,
                    twName = "蔡詣弘",
                    engName = "Eason",
                    email = "561993111e@gmail.com",
                    github = "EasonTsia",
                    img = "eason.jpg",
                    level = "組員",
                    description = "",
                    skill = new string[] {
                        "SCSS", "JS", "Tailwindcss", "Vue", "ASP.NET Core", "SQL"
                    },
                    works = new {
                        envs = new string[] {
                            "專案主題構想",
                            "資料庫架構構思",
                        },
                        pages = new string[] {
                            "前台:Search",
                            "前台:Notify",
                            "前台:Follows",
                            "前台:Collects",
                            "後台:Reports"
                        }
                    }
                },
                new {
                    index = 2,
                    twName = "林鈺棠",
                    engName = "",
                    email = "lin055377@gmail.com",
                    github = "Tommy1050",
                    img = "tommy.jpg",
                    level = "組員",
                    description = "",
                    skill = new string[] {
                        "SCSS", "JS", "Tailwindcss", "Vue", "ASP.NET Core", "SQL"
                    },
                    works = new {
                        envs = new string[] {
                            "專案主題構想",
                            "資料庫架構構思",
                        },
                        pages = new string[] {
                            "「個人檔案」頁面製作",
                            "「編輯個人檔案」浮動視窗製作",
                            "「帳號管理」後台頁面製作",
                            "「NFT 展示」頁面製作",
                        }
                    }
                },
                new {
                    index = 3,
                    twName = "黃韋傑",
                    engName = "Jay",
                    email = "weijay907@gmail.com",
                    github = "Jay9453",
                    img = "jay.jpg",
                    level = "組員",
                    description = "",
                    skill = new string[] {
                        "SCSS", "JS", "Tailwindcss", "Vue", "ASP.NET Core", "SQL"
                    },
                    works = new {
                        envs = new string[] {
                            "專案主題構想",
                            "資料庫架構構思",
                        },
                        pages = new string[] {
                            "動態圖片 顯示製作", 
                            "動態圖片 刪除製作", 
                            "好友列表 製作" 
                        }
                    }
                },
            };

            ViewBag.Teams = members;
            return View();
        }
    }
}