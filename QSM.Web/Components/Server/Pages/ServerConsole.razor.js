function scrollToBottom() {
    const list = document.getElementById("outputList");
    
    if (list === null) return;
    
    list.scrollTo(0, list.scrollHeight);
}