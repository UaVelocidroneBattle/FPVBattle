// Sortable tables: a header with data-sort="key" sorts the body rows by their data-key attribute.
// Add data-sort-type="number" for numeric keys. A row with class "no-match-row" stays last.
document.querySelectorAll('table').forEach(table => {
    const headers = table.querySelectorAll('thead th[data-sort]');
    if (headers.length === 0) return;

    const tbody = table.querySelector('tbody');
    const rows = Array.from(tbody.querySelectorAll('tr:not(.no-match-row)'));
    const anchor = tbody.querySelector('tr.no-match-row');
    let currentSort = { key: null, dir: 1 };

    headers.forEach(header => {
        header.addEventListener('click', () => {
            const key = header.dataset.sort;
            const isNumber = header.dataset.sortType === 'number';

            currentSort.dir = currentSort.key === key ? -currentSort.dir : 1;
            currentSort.key = key;

            headers.forEach(h => h.removeAttribute('aria-sort'));
            header.setAttribute('aria-sort', currentSort.dir === 1 ? 'ascending' : 'descending');

            const sorted = [...rows].sort((a, b) => {
                const valueA = a.dataset[key];
                const valueB = b.dataset[key];
                const cmp = isNumber
                    ? parseFloat(valueA) - parseFloat(valueB)
                    : valueA.localeCompare(valueB);

                return cmp * currentSort.dir;
            });

            sorted.forEach(row => tbody.insertBefore(row, anchor));
        });
    });
});

// Searchable select: <div class="combobox" data-combobox> holding an input and a ul.combobox-menu of
// li > button.dropdown-item options. Typing filters the options, picking one fills the input.
document.querySelectorAll('[data-combobox]').forEach(combobox => {
    const input = combobox.querySelector('input');
    const menu = combobox.querySelector('.combobox-menu');
    const options = Array.from(menu.querySelectorAll('.dropdown-item'));
    const empty = menu.querySelector('.combobox-empty');
    let active = -1;

    const visibleOptions = () => options.filter(option => !option.parentElement.hidden);

    function setActive(index) {
        const visible = visibleOptions();
        active = visible.length === 0 ? -1 : (index + visible.length) % visible.length;
        options.forEach(option => option.classList.remove('active'));

        if (active >= 0) {
            visible[active].classList.add('active');
            visible[active].scrollIntoView({ block: 'nearest' });
        }
    }

    // Under the input, or above it when there is more room there.
    function place() {
        const gap = 4;
        const rect = input.getBoundingClientRect();
        const below = window.innerHeight - rect.bottom - gap;
        const above = rect.top - gap;
        const openUp = below < 160 && above > below;

        menu.style.left = `${rect.left}px`;
        menu.style.width = `${rect.width}px`;
        menu.style.maxHeight = `${Math.min(240, (openUp ? above : below) - 8)}px`;
        menu.style.top = openUp ? 'auto' : `${rect.bottom + gap}px`;
        menu.style.bottom = openUp ? `${window.innerHeight - rect.top + gap}px` : 'auto';
    }

    function open() {
        const term = input.value.trim().toLowerCase();

        options.forEach(option => {
            option.parentElement.hidden = !option.textContent.toLowerCase().includes(term);
        });

        empty.hidden = visibleOptions().length > 0;
        menu.classList.add('show');
        place();
        setActive(-1);
    }

    const close = () => menu.classList.remove('show');

    const reposition = () => {
        if (menu.classList.contains('show')) place();
    };

    window.addEventListener('resize', reposition);
    window.addEventListener('scroll', reposition, true);

    function choose(option) {
        input.value = option.textContent.trim();
        close();
    }

    input.addEventListener('focus', open);
    input.addEventListener('input', open);

    input.addEventListener('keydown', event => {
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
            if (!menu.classList.contains('show')) open();
            setActive(active + (event.key === 'ArrowDown' ? 1 : -1));
        } else if (event.key === 'Enter' && active >= 0) {
            event.preventDefault();
            choose(visibleOptions()[active]);
        } else if (event.key === 'Escape' && menu.classList.contains('show')) {
            event.stopPropagation();
            close();
        }
    });

    // mousedown, not click: the input would lose focus and the menu would close before a click lands.
    menu.addEventListener('mousedown', event => {
        event.preventDefault();
        const option = event.target.closest('.dropdown-item');
        if (option) choose(option);
    });

    document.addEventListener('mousedown', event => {
        if (!combobox.contains(event.target)) close();
    });
});
