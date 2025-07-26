/**
 * 格式化工具 Hook
 * 提供日期格式化和相對時間顯示功能
 */

window.MatrixCore = window.MatrixCore || {};

window.MatrixCore.UseFormatting = () => {
    /**
     * 格式化日期
     */
    const formatDate = (date, lang = 'zh-TW') => {
        const dateObj = new Date(date);
        
        if (lang === 'en-US') {
            return dateObj.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } else {
            return dateObj.toLocaleDateString('zh-TW', {
                year: 'numeric',
                month: 'numeric',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    };

    /**
     * 相對時間顯示
     */
    const timeAgo = (date, lang = 'zh-TW') => {
        const now = new Date();
        const target = new Date(date);
        const diffInSeconds = Math.floor((now - target) / 1000);

        if (lang === "en-US") {
            if (diffInSeconds < 60) return 'Just now';
            if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} minutes ago`;
            if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
        } else {
            if (diffInSeconds < 60) return '剛剛';
            if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} 分鐘前`;
            if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} 小時前`;
        }
        
        const days = Math.floor(diffInSeconds / 86400);
        if (lang === "en-US") {
            if (days < 30) return `${days} days ago`;
            if (days < 365) return `${Math.floor(days / 30)} months ago`;
            return `${Math.floor(days / 365)} years ago`;
        } else {
            if (days < 30) return `${days} 天前`;
            if (days < 365) return `${Math.floor(days / 30)} 個月前`;
            return `${Math.floor(days / 365)} 年前`;
        }
    };

    /**
     * 格式化數字（添加千分位分隔符）
     */
    const formatNumber = (number) => {
        return new Intl.NumberFormat().format(number);
    };

    return {
        formatDate,
        timeAgo,
        formatNumber
    };
};