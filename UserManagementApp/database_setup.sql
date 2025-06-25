-- Скрипт создания базы данных PostgreSQL для приложения управления пользователями

-- Создание базы данных
-- CREATE DATABASE usermanagement;

-- Подключение к базе данных usermanagement
-- \c usermanagement;

-- Создание таблицы ролей
CREATE TABLE IF NOT EXISTS roles (
    role_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    permissions TEXT
);

-- Создание таблицы пользователей
CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role_id INTEGER NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP WITH TIME ZONE,
    FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

-- Индексы для улучшения производительности
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_role_id ON users(role_id);

-- Вставка начальных ролей
INSERT INTO roles (name, permissions) VALUES 
    ('Администратор', 'full_access,manage_users,manage_roles'),
    ('Модератор', 'moderate_content,view_users'),
    ('Пользователь', 'basic_access')
ON CONFLICT (name) DO NOTHING;

-- Вставка тестовых пользователей
INSERT INTO users (username, email, password_hash, role_id) VALUES 
    ('admin', 'admin@example.com', '', 1),
    ('moderator', 'moderator@example.com', '', 2),
    ('user1', 'user1@example.com', '', 3),
    ('user2', 'user2@example.com', '', 3)
ON CONFLICT (username) DO NOTHING;

-- Примеры запросов для проверки

-- Получение всех пользователей с ролями (основной запрос приложения)
-- SELECT u.user_id, u.username, u.email, r.name as role
-- FROM users u
-- JOIN roles r ON u.role_id = r.role_id;

-- Получение всех ролей
-- SELECT role_id, name, permissions FROM roles ORDER BY name;

-- Проверка существования пользователя
-- SELECT COUNT(*) FROM users WHERE username = 'admin' OR email = 'admin@example.com';
