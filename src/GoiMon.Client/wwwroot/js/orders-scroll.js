window.ordersScroll = {
  getMetrics: function (element) {
    if (!element) {
      return { scrollTop: 0, clientHeight: 0, scrollHeight: 0 };
    }

    return {
      scrollTop: element.scrollTop || 0,
      clientHeight: element.clientHeight || 0,
      scrollHeight: element.scrollHeight || 0
    };
  }
};
