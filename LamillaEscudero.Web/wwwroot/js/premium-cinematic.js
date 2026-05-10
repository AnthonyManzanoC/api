(function () {
    'use strict';

    var reduceMotion = window.matchMedia &&
        window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var revealObserver = null;
    var rafId = 0;
    var mutationTimer = 0;
    var decorated = new WeakSet();
    var sections = [];

    var revealSelectors = [
        '.reveal',
        '.hero-copy > *',
        '.hero-contact-card',
        '.about-grid > *',
        '.section-header',
        '.why-feature',
        '.trayectoria-copy',
        '.trayectoria-stats',
        '.practice-grid > *',
        '.testi-carousel',
        '.testi-form-card',
        '.value-item',
        '.cta-gold-inner',
        '.svc-hero-copy > *',
        '.svc-hero-panel',
        '.svc-overview-grid > *',
        '.servicios-acordeon > *',
        '.svc-bottom-band',
        '.team-directory-hero-grid > *',
        '.editorial-team-stack > *',
        '.team-directory-grid > *',
        '.team-directory-card-empty',
        '.cx-welcome-copy > *',
        '.cx-welcome-panel',
        '.cx-welcome-strip > *',
        '.cx-panel-heading > *',
        '.cx-hero > *',
        '.cx-form-shell',
        '.cx-info-card',
        '.cx-info-items > *',
        '.cx-process-step',
        '.cx-trust-item',
        '.cx-mission-heading > *',
        '.cx-mission-grid > *'
    ].join(',');

    var materialSelectors = [
        '.hero-contact-card',
        '.about-img-frame',
        '.practice-card-v2',
        '.why-feature',
        '.trayectoria-stats-v2',
        '.testimonial-card',
        '.testi-form-card',
        '.svc-hero-panel',
        '.svc-metric-card',
        '.svc-content-shell',
        '.servicio-item',
        '.svc-bottom-band',
        '.editorial-member',
        '.team-directory-sidecard',
        '.team-directory-card',
        '.team-directory-card-empty',
        '.cx-welcome-panel',
        '.cx-welcome-signal',
        '.cx-form-shell',
        '.cx-info-card',
        '.cx-success',
        '.cx-mission-card'
    ].join(',');

    function clamp(value, min, max) {
        return Math.max(min, Math.min(max, value));
    }

    function getPublicRoot() {
        return document.querySelector('.public-shell');
    }

    function getGroupIndex(el) {
        var parent = el.parentElement;
        if (!parent) return 0;
        var siblings = Array.prototype.filter.call(parent.children, function (child) {
            return child.matches && child.matches(revealSelectors);
        });
        return Math.max(0, siblings.indexOf(el));
    }

    function decorateReveal(el) {
        if (decorated.has(el)) return;
        decorated.add(el);
        el.classList.add('cinema-reveal');

        var index = getGroupIndex(el);
        var capped = Math.min(index, 8);
        el.style.setProperty('--cinema-delay', (capped * 72) + 'ms');

        if (reduceMotion) {
            el.classList.add('cinema-in-view', 'visible');
        }
    }

    function decorateMaterials(root) {
        root.querySelectorAll(materialSelectors).forEach(function (el) {
            el.classList.add('premium-material');
            if (!el.dataset.cinemaMaterial) {
                el.dataset.cinemaMaterial = '1';
                el.addEventListener('pointermove', function (event) {
                    var rect = el.getBoundingClientRect();
                    var x = ((event.clientX - rect.left) / rect.width) * 100;
                    var y = ((event.clientY - rect.top) / rect.height) * 100;
                    el.style.setProperty('--shine-x', clamp(x, 0, 100).toFixed(2) + '%');
                    el.style.setProperty('--shine-y', clamp(y, 0, 100).toFixed(2) + '%');
                    el.style.setProperty('--shine-angle', (104 + (x - 50) * .18).toFixed(2) + 'deg');
                }, { passive: true });
                el.addEventListener('pointerleave', function () {
                    el.style.removeProperty('--shine-x');
                    el.style.removeProperty('--shine-y');
                    el.style.removeProperty('--shine-angle');
                }, { passive: true });
            }
        });
    }

    function observeReveals(root) {
        if (revealObserver) revealObserver.disconnect();

        var items = root.querySelectorAll(revealSelectors);
        items.forEach(decorateReveal);

        if (reduceMotion) return;

        revealObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('cinema-in-view', 'visible');
                    entry.target.classList.remove('cinema-exiting');
                } else if (entry.boundingClientRect.top < 0) {
                    entry.target.classList.add('cinema-exiting');
                } else {
                    entry.target.classList.remove('cinema-exiting');
                }
            });
        }, {
            threshold: [0, .12, .28],
            rootMargin: '0px 0px -8% 0px'
        });

        items.forEach(function (el) { revealObserver.observe(el); });
    }

    function collectSections(root) {
        sections = Array.prototype.slice.call(root.querySelectorAll(
            '.hero-v2, .about-section, .team-section, .why-section, .trayectoria-section, .practice-section, .testimonials-section, .values-section, .cta-gold-band, .svc-hero, .svc-overview, .svc-section, .team-directory-hero, .team-directory-featured, .team-directory-grid-section, .cx-section'
            + ', .cx-welcome-section'
        ));
        sections.forEach(function (section) { section.classList.add('cinema-section'); });
    }

    function updateScrollState() {
        rafId = 0;
        var scrollY = window.scrollY || window.pageYOffset || 0;
        var height = window.innerHeight || 1;

        document.body.classList.toggle('cinema-scrolled', scrollY > 24);
        var globalProgress = Math.min(scrollY / 1200, 1);
        document.documentElement.style.setProperty('--cinema-scroll', globalProgress.toFixed(3));
        document.documentElement.style.setProperty('--cinema-metal-shift', (-40 + globalProgress * 8).toFixed(2) + '%');

        sections.forEach(function (section) {
            var rect = section.getBoundingClientRect();
            var progress = clamp((height - rect.top) / (height + rect.height), 0, 1);
            var centerDelta = Math.abs((rect.top + rect.height / 2) - height / 2);
            var focus = clamp(centerDelta / (height * .78), 0, 1);

            section.style.setProperty('--cinema-progress', progress.toFixed(3));
            section.style.setProperty('--cinema-lift', (progress * -22).toFixed(2) + 'px');
            section.style.setProperty('--cinema-grid-lift', (progress * -14).toFixed(2) + 'px');
            section.style.setProperty('--cinema-bg-lift', (progress * -28).toFixed(2) + 'px');
            section.style.setProperty('--cinema-content-lift', (progress * -10).toFixed(2) + 'px');

            var keepSharp = section.classList.contains('cx-section') ||
                section.classList.contains('cx-welcome-section');
            section.classList.toggle('cinema-soft-focus', !keepSharp && focus > .92 && rect.top < 0);

            if (section.classList.contains('hero-v2')) {
                var heroProgress = clamp(scrollY / Math.max(1, rect.height), 0, 1);
                section.style.setProperty('--hero-progress', heroProgress.toFixed(3));
                section.style.setProperty('--hero-copy-lift', (heroProgress * -20).toFixed(2) + 'px');
                section.style.setProperty('--hero-card-lift', (heroProgress * 18).toFixed(2) + 'px');
                section.style.setProperty('--hero-copy-opacity', (1 - heroProgress * .2).toFixed(3));
            }
        });
    }

    function requestScrollUpdate() {
        if (!rafId) rafId = window.requestAnimationFrame(updateScrollState);
    }

    function refresh() {
        var root = getPublicRoot();
        if (!root) return;

        document.body.classList.add('cinema-ready');
        if (reduceMotion) document.body.classList.add('cinema-reduced-motion');

        decorateMaterials(root);
        collectSections(root);
        observeReveals(root);
        requestScrollUpdate();
    }

    function scheduleRefresh() {
        window.clearTimeout(mutationTimer);
        mutationTimer = window.setTimeout(refresh, 80);
    }

    function boot() {
        refresh();
        window.addEventListener('scroll', requestScrollUpdate, { passive: true });
        window.addEventListener('resize', requestScrollUpdate, { passive: true });
        document.addEventListener('enhancedload', scheduleRefresh);

        var root = getPublicRoot();
        if (root && window.MutationObserver) {
            var observer = new MutationObserver(scheduleRefresh);
            observer.observe(root, { childList: true, subtree: true });
        }

        window.LamillaPremiumMotion = {
            refresh: refresh
        };
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot, { once: true });
    } else {
        boot();
    }
})();
