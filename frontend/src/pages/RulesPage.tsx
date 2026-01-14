const PageRules: React.FC = () => {
  return <>
    <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden p-6 max-w-4xl mx-auto">
      <div className="space-y-8 text-slate-200">
        <h2 className="text-xl font-bold text-emerald-400 mb-6">Інструкція:</h2>

        <ol className="list-decimal list-inside space-y-6">
          <li className="pl-2">
            <span>Качаємо Velocidrone з </span>
            <a href="https://www.velocidrone.com/shop"
              className="text-emerald-400 hover:text-emerald-300 transition-colors"
              target="_blank"
              rel="noopener noreferrer"
            >
              офіційного сайту
            </a>
          </li>

          <li className="pl-2">
            Встановлюємо автопублікацію результатів в налаштуваннях сімулятора:
            <br />
            <span className="text-slate-400 pl-4">{'Options -> Main Settings -> Auto Leaderbord Time Update: Yes'}</span>
          </li>

          <li className="pl-2">Обираємо один з режимів: Nemesis або Single Player</li>

          <li className="pl-2">Обираємо Game type: Single Class - 3 Laps</li>

          <li className="pl-2">
            Кожен день о 17:00 бот обирає випадковий трек із набору локацій та треків.
            Іноді може бути ініційована зміна треку, якщо він не буде підходити для змагань.
          </li>

          <li className="pl-2">
            Також важливо оцінювати кожен трек, для цього бот буде публікувати голосування.
            Результати голосування будуть впливати на те, чи вибере бот цей трек знову.
          </li>

          <li className="pl-2">
            Через добу о 17:00 бот підведе підсумки дня, нарахує бали в залежності від позиції.
          </li>

          <li className="pl-2">
            Сезон починається кожний місяць 1-го числа і закінчується 1-го числа наступного місяця.
          </li>

          <li className="pl-2">
            Також треба прослідкувати, щоб ваш акаунт в Velocidrone мав український прапор. Це можна подивитись і змінити, якщо треба, в вашому профайлі на сайті Velocidrone.
          </li>
        </ol>

        <h2 className="text-xl font-bold text-emerald-400">Підтримка</h2>

        <div>
          <p>Ви можете підтримати проєкт на нашому
            <a href="https://patreon.com/FPVBattle"
              className="text-emerald-400 hover:text-emerald-300 transition-colors"
              target="_blank"
              rel="noopener noreferrer"> Patreon</a> для покриття різних витрат (хостинг, домен, кава, тощо).</p>
        </div>

        <p className="text-xl font-bold text-emerald-400 text-center mt-8">Полетіли! 🚀</p>
      </div>
    </div>
  </>
}

export default PageRules;