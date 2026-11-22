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
// li > button.dropdown-item options. Typing filters the options, picking one fills the input with the
// option's data-value, or its text when it has none.
// A hidden input in the combobox receives the picked option's data-key. The text is then only a search box:
// until an option is picked, the form refuses to submit with data-combobox-pick-message.
// With data-combobox-source the options come from that URL instead: it gets the typed term and the form
// fields named in data-combobox-params, and returns [{ key, value, label, notes: [{ text, warning }] }].
// Nothing loads below data-combobox-min-length characters.
document.querySelectorAll('[data-combobox]').forEach(combobox => {
    const input = combobox.querySelector('input:not([type=hidden])');
    const keyInput = combobox.querySelector('input[type=hidden]');
    const menu = combobox.querySelector('.combobox-menu');
    const empty = menu.querySelector('.combobox-empty');
    const source = combobox.dataset.comboboxSource;
    const minLength = Number(combobox.dataset.comboboxMinLength ?? 0);
    const paramNames = (combobox.dataset.comboboxParams ?? '').split(' ').filter(Boolean);
    let options = Array.from(menu.querySelectorAll('.dropdown-item'));
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

        if (source && term.length < minLength) {
            close();
            return;
        }

        if (!source) {
            options.forEach(option => {
                option.parentElement.hidden = !option.textContent.toLowerCase().includes(term);
            });
        }

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
        input.value = option.dataset.value ?? option.textContent.trim();

        if (keyInput) {
            keyInput.value = option.dataset.key;
            input.setCustomValidity('');
        }

        close();
    }

    function forgetPick() {
        keyInput.value = '';
        input.setCustomValidity(combobox.dataset.comboboxPickMessage ?? 'Pick an option from the list');
    }

    function showOptions(items) {
        options.forEach(option => option.parentElement.remove());

        options = items.map(item => {
            const option = document.createElement('button');
            option.type = 'button';
            option.className = 'dropdown-item combobox-option';
            option.dataset.key = item.key;
            option.dataset.value = item.value;

            const label = document.createElement('span');
            label.className = 'combobox-label';
            label.textContent = item.label;
            option.append(label);

            (item.notes ?? []).forEach(({ text, warning }) => {
                const note = document.createElement('span');
                note.className = warning ? 'combobox-note combobox-note-warning' : 'combobox-note';
                note.textContent = text;
                option.append(note);
            });

            const row = document.createElement('li');
            row.append(option);
            menu.insertBefore(row, empty);

            return option;
        });
    }

    let latestRequest = 0;

    async function loadOptions() {
        const term = input.value.trim();
        const request = ++latestRequest;

        if (term.length < minLength) {
            showOptions([]);
            close();
            return;
        }

        const query = new URLSearchParams({ term });
        paramNames.forEach(name => query.set(name, input.form.elements[name].value));

        const response = await fetch(`${source}?${query}`);

        // A slower answer to an older term must not replace the newer one.
        if (request !== latestRequest || !response.ok) return;

        showOptions(await response.json());
        open();
    }

    let loadTimer;
    const loadSoon = () => {
        clearTimeout(loadTimer);
        loadTimer = setTimeout(loadOptions, 200);
    };

    input.addEventListener('focus', source ? loadOptions : open);
    input.addEventListener('input', source ? loadSoon : open);
    if (keyInput) input.addEventListener('input', forgetPick);

    // Options loaded for other params would be stale, so they reload on the next focus.
    paramNames.forEach(name => input.form.elements[name].addEventListener('change', () => showOptions([])));

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

// A select whose options depend on another field: <select data-options-follow="fieldName"> shows only
// the options whose data-for matches that field's value, plus those without data-for.
document.querySelectorAll('[data-options-follow]').forEach(select => {
    const field = select.form.elements[select.dataset.optionsFollow];

    function showMatchingOptions() {
        Array.from(select.options).forEach(option => {
            option.hidden = option.dataset.for !== undefined && option.dataset.for !== field.value;
        });

        if (select.selectedOptions[0]?.hidden) select.value = '';
    }

    field.addEventListener('change', showMatchingOptions);
    showMatchingOptions();
});
