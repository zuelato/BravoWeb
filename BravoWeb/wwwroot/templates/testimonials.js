(function(){
    const container = document.getElementById('testimonials-carousel');
    const track = container.querySelector('.carousel-track');
    const prevBtn = document.getElementById('carousel-prev');
    const nextBtn = document.getElementById('carousel-next');
    let cards = Array.from(track.children);
    let cardWidth = 0;
    let gapBetween = 0;
    let halfGap = 0;
    let currentTranslate = 0;
    let isAnimating = false;

    function recalcSizesOnly(){
        const viewport = window.innerWidth || document.documentElement.clientWidth || container.clientWidth;
        const computed = getComputedStyle(cards[0]);
        const ml = parseFloat(computed.marginLeft) || 0;
        const mr = parseFloat(computed.marginRight) || 0;
        gapBetween = ml + mr;
        halfGap = ml;
        cardWidth = (viewport - 3 * gapBetween) / 3;
        cards.forEach(c => { c.style.flex = '0 0 ' + cardWidth + 'px'; });
    }

    function restingOffset(){
        return -(cardWidth / 2 + halfGap);
    }

    function updateSizes(){
        recalcSizesOnly();
        currentTranslate = restingOffset();
        setTranslate(currentTranslate, false);
    }

    function setTranslate(x, animate){
        track.style.transition = animate ? 'transform 0.45s ease' : 'none';
        track.style.transform = 'translate3d(' + x + 'px,0,0)';
    }

    function moveNext(){
        if(isAnimating) return;
        isAnimating = true;
        nextBtn.style.pointerEvents = 'none';
        prevBtn.style.pointerEvents = 'none';
        var step = cardWidth + gapBetween;
        var target = currentTranslate - step;
        setTranslate(target, true);
        var onEnd = function(){
            track.removeEventListener('transitionend', onEnd);
            track.appendChild(track.firstElementChild);
            cards = Array.from(track.children);
            recalcSizesOnly();
            void track.offsetWidth;
            currentTranslate = restingOffset();
            setTranslate(currentTranslate, false);
            isAnimating = false;
            nextBtn.style.pointerEvents = '';
            prevBtn.style.pointerEvents = '';
        };
        track.addEventListener('transitionend', onEnd);
    }

    function movePrev(){
        if(isAnimating) return;
        isAnimating = true;
        nextBtn.style.pointerEvents = 'none';
        prevBtn.style.pointerEvents = 'none';
        var step = cardWidth + gapBetween;
        track.insertBefore(track.lastElementChild, track.firstChild);
        cards = Array.from(track.children);
        recalcSizesOnly();
        currentTranslate = restingOffset() - step;
        setTranslate(currentTranslate, false);
        void track.offsetWidth;
        var target = restingOffset();
        setTranslate(target, true);
        var onEnd = function(){
            track.removeEventListener('transitionend', onEnd);
            currentTranslate = target;
            cards = Array.from(track.children);
            isAnimating = false;
            nextBtn.style.pointerEvents = '';
            prevBtn.style.pointerEvents = '';
        };
        track.addEventListener('transitionend', onEnd);
    }

    nextBtn.addEventListener('click', moveNext);
    prevBtn.addEventListener('click', movePrev);
    window.addEventListener('resize', updateSizes);
    updateSizes();
})();
