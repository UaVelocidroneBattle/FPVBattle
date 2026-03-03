const PageRules: React.FC = () => {
  return <>
    <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden p-6 max-w-4xl mx-auto">
      <div className="space-y-8 text-slate-200">
        <h2 className="text-xl font-bold text-emerald-400 mb-6">Інструкція</h2>

        <ol className="list-decimal list-outside pl-6 space-y-6">
          <li>
            <span>Завантажуємо Velocidrone з </span>
            <a href="https://www.velocidrone.com/shop"
              className="text-emerald-400 hover:text-emerald-300 transition-colors"
              target="_blank"
              rel="noopener noreferrer"
            >
              офіційного сайту
            </a>
          </li>

          <li>
            <span>Перевіряємо, щоб у вашому акаунті Velocidrone був встановлений український прапор. Це можна переглянути й змінити (за потреби) у вашому </span>
            <a href="https://www.velocidrone.com/profile"
              className="text-emerald-400 hover:text-emerald-300 transition-colors"
              target="_blank"
              rel="noopener noreferrer"
            >
              профілі
            </a>
          </li>
          <li>
            Вмикаємо автоматичну публікацію результатів у налаштуваннях симулятора:
            <br />
            <span className="text-slate-400">{'Options → Main Settings → Auto Leaderbord Time Update: Yes'}</span>
          </li>

          <li><span>Обираємо один з режимів: Nemesis або Single Player</span></li>

          <li><span>Обираємо Game type: Single Class – Three Laps Race</span></li>
        </ol>

        <h2 className="text-xl font-bold text-emerald-400 mb-6">Як це працює</h2>

        <ul className="list-disc list-outside pl-6 space-y-6">
          <li>
            <span>Сервіс автоматично оновлює результати кожні 10 хвилин і підтягне ваш результат, якщо він з'явиться.</span>
          </li>
          <li>
            <span>Щодня о 17:00 сервіс випадковим чином обирає трек із доступного набору локацій і треків. У деяких випадках трек може бути змінений, якщо він не підходить для змагань.</span>
          </li>
          <li>
            <span>Через 24 години, о 17:00, сервіс підбиває підсумки дня та нараховує бали відповідно до зайнятого місця.</span>
          </li>
          <li>
            <span>Сезон починається 1-го числа кожного місяця і завершується 1-го числа наступного місяця.</span>
          </li>
          <li>
            <span>Дуже важливо оцінювати кожен трек. Для цього сервіс публікує голосування (наразі тільки в Telegram-каналі). Результати голосування впливають на те, чи буде цей трек обраний знову.</span>
          </li>
        </ul>

        <h2 className="text-xl font-bold text-emerald-400 mb-6">Day streak</h2>

        <p>У системі працює механіка day streak - лічильник днів, протягом яких ви літаєте без перерв.</p>

        <ul className="list-disc list-outside pl-6 space-y-2">
          <li>
            <span>Якщо ви пропускаєте день - streak скидається.</span>
          </li>
          <li>
            <span>Якщо у вас є freeze, streak не скидається, але витрачається freeze.</span>
          </li>
        </ul>

        <h2 className="text-xl font-bold text-emerald-400 mb-6">Day streak freeze</h2>

        <div className="space-y-2">
          <p>Freeze (заморозки) потрібні, щоб зберегти ваш day streak у разі пропуску дня.</p>
          <p>Логіка нарахування:</p>
          <ul className="list-disc list-outside pl-6 space-y-2">
            <li><span>1 freeze за кожні 30 днів streak</span></li>
            <li>
              <span>або ви можете стати нашим підписником на </span>
              <a href="https://patreon.com/FPVBattle"
                className="text-emerald-400 hover:text-emerald-300 transition-colors"
                target="_blank"
                rel="noopener noreferrer"> Patreon</a>
              <span> - патронам нараховуються додаткові фрізи.</span>
            </li>
          </ul>
        </div>

        <h2 className="text-xl font-bold text-emerald-400 mb-6">Achievements</h2>
        <p>У системі є набір achievements, який постійно розширюється. Вони видаються за досягнення певної кількості day streak, першого місця в гонці, тощо.</p>

        <h2 className="text-xl font-bold text-emerald-400 mb-6">Підтримати нас</h2>

        <p>Ви можете підтримати проєкт на нашому
          <a href="https://patreon.com/FPVBattle"
            className="text-emerald-400 hover:text-emerald-300 transition-colors"
            target="_blank"
            rel="noopener noreferrer"> Patreon</a> для покриття різних витрат (хостинг, домен, кава, тощо).</p>

        <p className="text-xl font-bold text-emerald-400 text-center mt-8">Полетіли! 🚀</p>
      </div>
    </div>
  </>
}

export default PageRules;