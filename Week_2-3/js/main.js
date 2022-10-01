const navBtn = document.querySelector('#nav_button');
const sideNav = document.querySelector('#side_nav');
const respPage = document.querySelector('.responsive_page');
const main = document.querySelector('main');

navBtn.addEventListener('click', function () {
    sideNav.classList.toggle('side_nav_active');
    respPage.classList.toggle('responsive_page_overlay');
    main.classList.toggle('fixed');
})

document.addEventListener('click', function(event) {
    if (!navBtn.contains(event.target))
    {
        const sideNavActive = document.querySelector('.side_nav_active');
        if( !sideNavActive.contains(event.target)) {
            sideNav.classList.toggle('side_nav_active');
            respPage.classList.toggle('responsive_page_overlay');
            main.classList.toggle('fixed');
        } 
    }
});
