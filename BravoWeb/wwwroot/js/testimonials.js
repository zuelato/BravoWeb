    (function(){
        const container = document.getElementById('testimonials-carousel');
    const track = container.querySelector('.carousel-track');
    const prevBtn = document.getElementById('carousel-prev');
    const nextBtn = document.getElementById('carousel-next');
    let cards = Array.from(track.children);
    let cardWidth = 0;
    let gapBetween = 0;
    let currentTranslate = 0;
    let isAnimating = false;

    function recalcSizesOnly(){
            // Use viewport width so carousel truly bleeds edge-to-edge
            const viewport = window.innerWidth || document.documentElement.clientWidth || container.clientWidth;
    const trackStyle = getComputedStyle(track);
    const trackPadLeft = parseFloat(trackStyle.paddingLeft) || 0;
    const trackPadRight = parseFloat(trackStyle.paddingRight) || 0;
    const effectiveViewport = viewport - trackPadLeft - trackPadRight;

    const computed = getComputedStyle(cards[0]);
    const ml = parseFloat(computed.marginLeft) || 0;
    const mr = parseFloat(computed.marginRight) || 0;
    gapBetween = ml + mr;
    const totalGap = (3 - 1) * gapBetween;
    cardWidth = (effectiveViewport - totalGap) / 3;
            cards.forEach(c => {c.style.flex = `0 0 ${cardWidth}px`; });
        }

    function updateSizes(){
        recalcSizesOnly();
    // initial translate: negative half card so half shows left (preserves visual layout)
    currentTranslate = -cardWidth/2;
    setTranslate(currentTranslate, false);
        }

    function setTranslate(x, animate=true){
        track.style.transition = animate ? 'transform 0.45s ease' : 'none';
    track.style.transform = `translate3d(${x}px,0,0)`;
        }

    function moveNext(){
            if(isAnimating) return;
    isAnimating = true;
    nextBtn.style.pointerEvents = 'none';
    prevBtn.style.pointerEvents = 'none';
    const step = cardWidth + gapBetween;
    const target = currentTranslate - step;
    setTranslate(target, true);
            const onEnd = () => {
        track.removeEventListener('transitionend', onEnd);
    const first = track.firstElementChild;
    track.appendChild(first);
    // refresh cards and recalc sizes
    cards = Array.from(track.children);
    recalcSizesOnly();
    // force reflow
    void track.offsetWidth;
    // restore translate to original half card position
    currentTranslate = -cardWidth/2;
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
    const step = cardWidth + gapBetween;
    const last = track.lastElementChild;
    track.insertBefore(last, track.firstChild);
    // refresh cards and recalc sizes
    cards = Array.from(track.children);
    recalcSizesOnly();
    // set translate to show half card shifted left by one step
    currentTranslate = -(cardWidth/2) - step;
    setTranslate(currentTranslate, false);
    // force reflow
    void track.offsetWidth;
    // animate back to half card position
    const target = -cardWidth/2;
    setTranslate(target, true);
            const onEnd = () => {
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