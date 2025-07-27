function useFormatting() {
    const formatDate = (date, type = 'date', lang = 'zh-TW') => {
        if (!date) return '';
        
        const dateObj = new Date(date);
        if (isNaN(dateObj.getTime())) return '';
        
        // Simple date formatting without external dependencies
        const year = dateObj.getFullYear();
        const month = String(dateObj.getMonth() + 1).padStart(2, '0');
        const day = String(dateObj.getDate()).padStart(2, '0');
        const hours = String(dateObj.getHours()).padStart(2, '0');
        const minutes = String(dateObj.getMinutes()).padStart(2, '0');
        const ampm = dateObj.getHours() >= 12 ? 'PM' : 'AM';
        const engMonths = [{"01": "Jan"}, {"02": "Feb"}, {"03": "Mar"}, {"04": "Apr"}, {"05": "May"}, {"06": "Jun"}, {"07": "Jul"}, {"08": "Aug"}, {"09": "Sep"}, {"10": "Oct"}, {"11": "Nov"}, {"12": "Dec"}]
        const formattedDate = lang === 'en-US' ? `${engMonths[month]} ${day} ${year}` : `${year} 年 ${month} 月 ${day} 日`

        if (type === 'date') {
            return formattedDate
        } else {
            return `${formattedDate} ${hours}:${minutes} ${ampm}`
        }
    };

    const timeAgo = ({date, lang = "zh-TW"}) => {
        if (!date) return '';
        
        const now = new Date();
        const past = new Date(date);
        const diffInSeconds = Math.floor((now - past) / 1000);
        const days = Math.floor(diffInSeconds / 86400);

        if (diffInSeconds < 60) {
            return lang === "en-US" ? 'Just now' : '剛剛';
        } else if (diffInSeconds < 3600) {
            const minutes = Math.floor(diffInSeconds / 60);
            return lang === "en-US" ? `${minutes} minutes ago` : `${minutes} 分鐘前`;
        } else if (diffInSeconds < 86400) {
            const hours = Math.floor(diffInSeconds / 3600);
            return lang === "en-US" ? `${hours} hours ago` : `${hours} 小時前`;
        }
        
        if (days < 30) {
            return lang === "en-US" ? `${days} days ago` : `${days} 天前`;
        } else if (days < 365) {
            const months = Math.floor(days / 30);
            return lang === "en-US" ? `${months} months ago` : `${months} 個月前`;
        }

        const years = Math.floor(days / 365);
        return lang === "en-US" ? `${years} years ago` : `${years} 年前`;
    };

    return {
        formatDate,
        timeAgo
    };
}