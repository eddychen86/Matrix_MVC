function useFormatting() {
    const formatDate = (date, formatStr = 'yyyy-MM-dd') => {
        if (!date) return '';
        
        const dateObj = new Date(date);
        if (isNaN(dateObj.getTime())) return '';
        
        // Simple date formatting without external dependencies
        const year = dateObj.getFullYear();
        const month = String(dateObj.getMonth() + 1).padStart(2, '0');
        const day = String(dateObj.getDate()).padStart(2, '0');
        
        return `${year}-${month}-${day}`;
    };

    const timeAgo = (date) => {
        if (!date) return '';
        
        const now = new Date();
        const past = new Date(date);
        const diffInSeconds = Math.floor((now - past) / 1000);
        
        if (diffInSeconds < 60) return '剛剛';
        if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} 分鐘前`;
        if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} 小時前`;
        
        const days = Math.floor(diffInSeconds / 86400);
        if (days < 30) return `${days} 天前`;
        if (days < 365) return `${Math.floor(days / 30)} 個月前`;
        return `${Math.floor(days / 365)} 年前`;
    };

    return {
        formatDate,
        timeAgo
    };
}