window.teaprendoChat = {
    scrollToEnd(element) {
        if (element) {
            element.scrollTo({
                top: element.scrollHeight,
                behavior: "smooth"
            });
        }
    }
};
