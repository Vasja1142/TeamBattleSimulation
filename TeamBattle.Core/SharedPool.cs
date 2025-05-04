// Файл: TeamBattle.Core/SharedPool.cs
using System;

namespace TeamBattle.Core
{
    /// <summary>
    /// Представляет общий пул бойцов, доступных для найма командами.
    /// Обеспечивает потокобезопасный доступ.
    /// </summary>
    public class SharedPool
    {
        private int _availableFighters;
        private readonly object _poolLock = new object(); // Объект для синхронизации доступа

        /// <summary>
        /// Получает текущее количество доступных бойцов в пуле.
        /// Доступ потокобезопасен.
        /// </summary>
        public int AvailableFighters
        {
            get
            {
                lock (_poolLock)
                {
                    return _availableFighters;
                }
            }
            private set // Сеттер приватный, изменяется только через TryTakeFighters
            {
                lock (_poolLock)
                {
                    _availableFighters = value;
                }
            }
        }

        /// <summary>
        /// Инициализирует пул указанным количеством бойцов.
        /// </summary>
        /// <param name="initialSize">Начальное количество бойцов.</param>
        public SharedPool(int initialSize)
        {
            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), "Начальный размер пула не может быть отрицательным.");
            _availableFighters = initialSize;
        }

        /// <summary>
        /// Пытается взять указанное количество бойцов из пула.
        /// Метод потокобезопасен.
        /// </summary>
        /// <param name="requested">Запрошенное количество бойцов.</param>
        /// <param name="taken">Реально взятое количество бойцов (может быть меньше запрошенного или 0).</param>
        /// <returns>True, если удалось взять хотя бы одного бойца, иначе false.</returns>
        public bool TryTakeFighters(int requested, out int taken)
        {
            if (requested <= 0)
            {
                taken = 0;
                return false;
            }

            lock (_poolLock) // Блокируем доступ к пулу на время операции
            {
                if (_availableFighters <= 0)
                {
                    taken = 0;
                    return false; // Пул пуст
                }

                // Определяем, сколько можем реально взять
                taken = Math.Min(requested, _availableFighters);
                _availableFighters -= taken; // Уменьшаем количество в пуле

                return taken > 0;
            } // Блокировка снимается здесь
        }

        /// <summary>
        /// Возвращает строковое представление состояния пула.
        /// </summary>
        public override string ToString()
        {
            return $"Пул: {AvailableFighters} бойцов";
        }
    }
}