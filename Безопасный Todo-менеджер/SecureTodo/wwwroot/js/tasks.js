document.addEventListener('click', async event => {
  const button = event.target.closest('.complete-button');
  if (!button) return;
  const token = document.querySelector('meta[name="csrf-token"]')?.content;
  if (!token) return;
  button.disabled = true;
  const oldText = button.textContent;
  button.textContent = 'Сохраняем…';
  try {
    const response = await fetch(`/tasks?handler=Complete&id=${encodeURIComponent(button.dataset.taskId)}`, {
      method: 'POST',
      headers: { 'X-CSRF-TOKEN': token }
    });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const result = await response.json();
    if (!result.isCompleted) throw new Error('Задача не была обновлена.');
    const card = button.closest('.task-card');
    card?.querySelector('.task-check')?.classList.add('done');
    const check = card?.querySelector('.task-check');
    if (check) check.textContent = '✓';
    card?.querySelector('h2')?.classList.add('completed');
    const status = card?.querySelector('.task-status');
    if (status) status.textContent = 'Готово';
    button.remove();
  } catch (error) {
    button.disabled = false;
    button.textContent = oldText;
    alert('Не удалось выполнить задачу. Проверьте соединение и права доступа.');
    console.error(error);
  }
});
