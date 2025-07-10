// ================================ Date Format Helpers ==============================

function formatDate(dotNetDateStr) {
    if (!dotNetDateStr) return '';
    const match = /\/Date\((\d+)(?:-\d+)?\)\//.exec(dotNetDateStr);
    if (!match) return dotNetDateStr;
    const date = new Date(parseInt(match[1]));
    return `${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getDate().toString().padStart(2, '0')}/${date.getFullYear()}`;
}

function formatDotNetDate(dotNetDateStr) {
    if (!dotNetDateStr) return null;
    const timestamp = parseInt(dotNetDateStr.replace(/\/Date\((\d+)\)\//, "$1"));
    const date = new Date(timestamp);
    return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
}

function getCurrentDate() {
    const date = new Date();
    return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
}

function formatDateToDisplay(dateString) {
    const date = new Date(dateString);
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-based
    const day = String(date.getDate()).padStart(2, '0');
    const year = date.getFullYear();
    return `${year}-${month}-${day}`;
}