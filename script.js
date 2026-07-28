document.addEventListener("DOMContentLoaded", function () {
  const menuToggle = document.querySelector(".site-menu-toggle");
  const nav = document.querySelector(".site-nav");
  const navLinks = document.querySelectorAll(".site-nav a");

  if (!menuToggle || !nav) {
    return;
  }

  menuToggle.addEventListener("click", function () {
    const isOpen = nav.classList.toggle("open");
    menuToggle.setAttribute("aria-expanded", String(isOpen));
    menuToggle.setAttribute(
      "aria-label",
      isOpen ? "Close navigation" : "Open navigation",
    );
  });

  navLinks.forEach((link) => {
    link.addEventListener("click", function () {
      if (nav.classList.contains("open")) {
        nav.classList.remove("open");
        menuToggle.setAttribute("aria-expanded", "false");
        menuToggle.setAttribute("aria-label", "Open navigation");
      }
    });
  });
});
