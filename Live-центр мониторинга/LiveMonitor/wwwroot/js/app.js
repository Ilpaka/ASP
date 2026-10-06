(() => {
  'use strict';

  if (location.protocol === 'file:') {
    const status = document.getElementById('connectionStatus');
    status.className = 'connection-status offline';
    status.querySelector('span').textContent = 'Для связи запустите сервер';
    document.getElementById('myConnectionId').textContent = 'Нужен dotnet run';
    return;
  }

  const $ = id => document.getElementById(id);
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/monitor')
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  let currentRoom = '';
  let latestUsers = [];
  let eventCount = 0;
  const typingTimers = new WeakMap();
  let lastRoomTypingSent = 0;
  let lastBroadcastTypingSent = 0;
  let toastTimer;
  let streamSubscription;
  let streamValues = [];
  let streamMaximum = 100;

  function setStatus(state, text) {
    const status = $('connectionStatus');
    status.className = `connection-status ${state}`;
    status.querySelector('span').textContent = text;
  }

  function notify(text) {
    const toast = $('toast');
    toast.textContent = text;
    toast.classList.add('visible');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('visible'), 3200);
  }

  function shortId(id) {
    if (!id) return 'неизвестный';
    return id === 'HTTP' ? 'HTTP' : id.slice(0, 8) + '…';
  }

  function addEvent(type, title, text, label = '') {
    $('feedEmpty')?.remove();
    const item = document.createElement('article');
    item.className = `event-item ${type}`;
    const marker = document.createElement('span');
    marker.className = 'event-marker';
    marker.textContent = { broadcast: '✦', room: '#', private: '↗', system: '✧', presence: '◉' }[type] || '•';
    const body = document.createElement('div');
    body.className = 'event-body';
    const top = document.createElement('div');
    top.className = 'event-top';
    const titleNode = document.createElement('strong');
    titleNode.textContent = title;
    if (label) {
      const badge = document.createElement('span');
      badge.className = 'event-label';
      badge.textContent = label;
      titleNode.append(badge);
    }
    const time = document.createElement('time');
    time.textContent = new Intl.DateTimeFormat('ru-RU', { hour: '2-digit', minute: '2-digit', second: '2-digit' }).format(new Date());
    top.append(titleNode, time);
    const message = document.createElement('p');
    message.textContent = text;
    body.append(top, message);
    item.append(marker, body);
    $('eventFeed').prepend(item);
    eventCount++;
    $('eventCount').textContent = String(eventCount);
    while ($('eventFeed').children.length > 150) $('eventFeed').lastElementChild.remove();
  }

  function showRoom(room) {
    const tag = $('currentRoomTag');
    tag.textContent = room || 'Не в комнате';
    tag.classList.toggle('active', !!room);
    if (room) {
      $('roomName').value = room;
      $('externalRoom').value = room;
    }
  }

  function renderUsers(users) {
    latestUsers = Array.isArray(users) ? users : [];
    $('userCount').textContent = String(latestUsers.length);
    const list = $('userList');
    list.replaceChildren();
    if (!latestUsers.length) {
      const empty = document.createElement('div');
      empty.className = 'list-empty';
      empty.textContent = 'Пока никого нет в сети.';
      list.append(empty);
      return;
    }
    for (const user of latestUsers) {
      const own = user.connectionId === connection.connectionId;
      const button = document.createElement('button');
      button.className = 'user-row';
      button.type = 'button';
      button.title = 'Подставить ID в поле получателя';
      const avatar = document.createElement('span');
      avatar.className = `user-avatar${own ? '' : ' other'}`;
      avatar.textContent = own ? 'ВЫ' : '●';
      const copy = document.createElement('span');
      copy.className = 'user-text';
      const name = document.createElement('strong');
      name.textContent = own ? 'Вы' : `Участник ${shortId(user.connectionId)}`;
      const id = document.createElement('small');
      id.textContent = user.connectionId;
      copy.append(name, id);
      const room = document.createElement('span');
      room.className = 'user-room';
      room.textContent = user.room || 'без комнаты';
      button.append(avatar, copy, room);
      button.addEventListener('click', () => {
        $('privateTarget').value = user.connectionId;
        $('privateText').focus();
        notify('ID участника подставлен в поле «Кому».');
      });
      list.append(button);
    }
  }

  async function invoke(method, ...args) {
    if (connection.state !== signalR.HubConnectionState.Connected) {
      notify('Нет соединения с сервером.');
      return false;
    }
    try {
      await connection.invoke(method, ...args);
      return true;
    } catch (error) {
      notify(error.message || 'Не удалось выполнить действие.');
      console.error(error);
      return false;
    }
  }

  async function post(url, payload) {
    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      if (!response.ok) {
        let problem;
        try { problem = await response.json(); } catch { /* пустой ответ */ }
        throw new Error(problem?.detail || `HTTP ${response.status}`);
      }
      return true;
    } catch (error) {
      notify(error.message || 'HTTP-запрос не выполнен.');
      console.error(error);
      return false;
    }
  }

  connection.on('UserConnected', id => addEvent('presence', 'Новый участник', `Подключился ${shortId(id)}`));
  connection.on('UserDisconnected', id => addEvent('presence', 'Участник отключился', `Отключился ${shortId(id)}`));
  connection.on('MessageReceived', (senderId, text) => addEvent('broadcast', `Участник ${shortId(senderId)}`, text, 'Всем'));
  connection.on('JoinedRoom', room => {
    currentRoom = room;
    lastRoomTypingSent = 0;
    showRoom(room);
    addEvent('room', 'Вы вошли в комнату', `Комната «${room}»`, 'Комната');
  });
  connection.on('LeftRoom', room => {
    currentRoom = '';
    $('typingIndicator').textContent = '';
    showRoom('');
    addEvent('room', 'Вы вышли из комнаты', `Комната «${room}»`, 'Комната');
  });
  connection.on('RoomMessageReceived', (senderId, room, text) =>
    addEvent('room', senderId === 'HTTP' ? 'Внешнее уведомление' : `Участник ${shortId(senderId)}`, text, `#${room}`));
  connection.on('OnlineCountUpdated', count => $('onlineCount').textContent = String(count));
  connection.on('UsersUpdated', renderUsers);
  connection.on('SystemMessage', text => addEvent('system', 'Система', text, 'Система'));
  connection.on('PrivateMessageReceived', (senderId, text) => addEvent('private', `От ${shortId(senderId)}`, text, 'Приватное'));
  connection.on('PrivateMessageSent', (recipientId, text) => addEvent('private', `Для ${shortId(recipientId)}`, text, 'Отправлено'));
  connection.on('UserTyping', (senderId, room) => {
    const indicator = room
      ? (room === currentRoom ? $('typingIndicator') : null)
      : $('broadcastTypingIndicator');
    if (!indicator) return;
    indicator.textContent = `Пользователь ${shortId(senderId)} печатает…`;
    clearTimeout(typingTimers.get(indicator));
    typingTimers.set(indicator, setTimeout(() => indicator.textContent = '', 2500));
  });

  connection.onreconnecting(() => {
    setStatus('connecting', 'Переподключение…');
    $('myConnectionId').textContent = 'Переподключение…';
    stopStream();
  });
  connection.onreconnected(async () => {
    setStatus('connected', 'Подключено');
    $('myConnectionId').textContent = connection.connectionId || '—';
    renderUsers(latestUsers);
    addEvent('presence', 'Связь восстановлена', 'Вы снова подключены к серверу.');
    if (currentRoom) await invoke('JoinRoom', currentRoom);
  });
  connection.onclose(() => {
    setStatus('offline', 'Соединение потеряно');
    $('myConnectionId').textContent = 'Нет соединения';
    $('onlineCount').textContent = '0';
    stopStream();
  });

  $('broadcastForm').addEventListener('submit', async event => {
    event.preventDefault();
    const text = $('broadcastText').value.trim();
    if (!text) return notify('Введите сообщение.');
    if (await invoke('SendMessage', text)) $('broadcastText').value = '';
  });

  $('broadcastText').addEventListener('input', () => {
    if (!$('broadcastText').value.trim()) return;
    const now = Date.now();
    if (now - lastBroadcastTypingSent < 1000) return;
    lastBroadcastTypingSent = now;
    void invoke('TypingGlobal');
  });

  $('joinRoom').addEventListener('click', async () => {
    const room = $('roomName').value.trim();
    if (!room) return notify('Введите название комнаты.');
    await invoke('JoinRoom', room);
  });

  $('leaveRoom').addEventListener('click', async () => {
    if (!currentRoom) return notify('Вы пока не вошли в комнату.');
    await invoke('LeaveRoom', currentRoom);
  });

  $('roomForm').addEventListener('submit', async event => {
    event.preventDefault();
    const text = $('roomText').value.trim();
    if (!currentRoom) return notify('Сначала войдите в комнату.');
    if (!text) return notify('Введите сообщение для комнаты.');
    if (await invoke('SendRoomMessage', currentRoom, text)) $('roomText').value = '';
  });

  $('roomText').addEventListener('input', () => {
    if (!currentRoom || !$('roomText').value.trim()) return;
    const now = Date.now();
    if (now - lastRoomTypingSent < 1000) return;
    lastRoomTypingSent = now;
    void invoke('Typing', currentRoom);
  });

  $('privateForm').addEventListener('submit', async event => {
    event.preventDefault();
    const target = $('privateTarget').value.trim();
    const text = $('privateText').value.trim();
    if (!target || !text) return notify('Укажите получателя и текст сообщения.');
    if (await invoke('SendPrivateMessage', target, text)) $('privateText').value = '';
  });

  $('externalRoomForm').addEventListener('submit', async event => {
    event.preventDefault();
    const room = $('externalRoom').value.trim();
    const text = $('externalText').value.trim();
    if (!room || !text) return notify('Укажите комнату и текст уведомления.');
    if (await post('/api/notifications/room', { room, text })) {
      $('externalText').value = '';
      notify('Уведомление отправлено в комнату.');
    }
  });

  $('sendSystem').addEventListener('click', async () => {
    if (await post('/api/notifications/system', { text: 'Системное уведомление из HTTP-запроса' }))
      notify('Системное сообщение отправлено всем.');
  });

  $('copyId').addEventListener('click', async () => {
    if (!connection.connectionId) return notify('ID появится после подключения.');
    try {
      await navigator.clipboard.writeText(connection.connectionId);
      notify('ID подключения скопирован.');
    } catch { notify('Не удалось скопировать ID. Выделите его вручную.'); }
  });

  $('clearFeed').addEventListener('click', () => {
    $('eventFeed').replaceChildren();
    eventCount = 0;
    $('eventCount').textContent = '0';
    const empty = document.createElement('div');
    empty.id = 'feedEmpty';
    empty.className = 'feed-empty';
    empty.innerHTML = '<div class="empty-symbol">◎</div><strong>Пока нет событий</strong><span>Новые события появятся здесь.</span>';
    $('eventFeed').append(empty);
  });

  function renderStream() {
    const chart = $('streamChart');
    chart.replaceChildren();
    for (const value of streamValues) {
      const bar = document.createElement('div');
      bar.className = 'stream-bar';
      bar.style.height = `${Math.max(4, value / streamMaximum * 100)}%`;
      bar.title = String(value);
      chart.append(bar);
    }
    $('lastNumber').textContent = streamValues.length ? String(streamValues.at(-1)) : '—';
  }

  function stopStream() {
    if (streamSubscription) {
      streamSubscription.dispose();
      streamSubscription = null;
    }
    $('startStream').disabled = false;
    $('stopStream').disabled = true;
    $('streamState').parentElement.classList.remove('active');
    $('streamState').innerHTML = '<i></i> Остановлен';
  }

  $('startStream').addEventListener('click', () => {
    if (connection.state !== signalR.HubConnectionState.Connected) return notify('Нет соединения с сервером.');
    const max = Number($('streamMax').value);
    if (!Number.isInteger(max) || max < 1 || max > 1000) return notify('Максимум должен быть от 1 до 1000.');
    stopStream();
    streamMaximum = max;
    streamValues = [];
    renderStream();
    $('startStream').disabled = true;
    $('stopStream').disabled = false;
    $('streamState').parentElement.classList.add('active');
    $('streamState').innerHTML = '<i></i> Поток активен';
    streamSubscription = connection.stream('StreamNumbers', max).subscribe({
      next: value => {
        streamValues.push(value);
        if (streamValues.length > 35) streamValues.shift();
        renderStream();
      },
      error: error => { console.error(error); notify('Поток прерван.'); stopStream(); },
      complete: stopStream
    });
  });
  $('stopStream').addEventListener('click', stopStream);

  async function start() {
    try {
      await connection.start();
      setStatus('connected', 'Подключено');
      $('myConnectionId').textContent = connection.connectionId || '—';
      renderUsers(latestUsers);
      addEvent('presence', 'Подключено', 'Вы подключились к Live-центру мониторинга.');
    } catch (error) {
      setStatus('offline', 'Соединение потеряно');
      $('myConnectionId').textContent = 'Нет соединения';
      notify('Не удалось подключиться. Перезагрузите страницу для повторной попытки.');
      console.error(error);
    }
  }

  start();
})();
