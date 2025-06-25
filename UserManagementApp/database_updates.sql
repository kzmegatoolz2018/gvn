-- Инкрементальные обновления базы данных

-- Версия 1.0.1 - Добавление индексов для улучшения производительности
-- CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(created_at DESC);  
-- CREATE INDEX IF NOT EXISTS idx_users_last_login ON users(last_login DESC);

-- Версия 1.0.2 - Добавление проверочных ограничений
-- ALTER TABLE users ADD CONSTRAINT check_username_length CHECK (LENGTH(username) >= 3);
-- ALTER TABLE users ADD CONSTRAINT check_email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');

-- Версия 1.0.3 - Добавление триггеров для автоматического обновления updated_at
-- CREATE OR REPLACE FUNCTION update_updated_at_column()
-- RETURNS TRIGGER AS $$
-- BEGIN
--     NEW.updated_at = CURRENT_TIMESTAMP;
--     RETURN NEW;
-- END;
-- $$ language 'plpgsql';
-- 
-- CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users 
--     FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Версия 1.0.4 - Добавление аудита изменений
-- CREATE TABLE IF NOT EXISTS user_audit (
--     audit_id SERIAL PRIMARY KEY,
--     user_id INTEGER NOT NULL,
--     operation VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
--     old_values JSONB,
--     new_values JSONB,
--     changed_by VARCHAR(50),
--     changed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
-- );

-- Версия 1.0.5 - Добавление настроек системы
-- CREATE TABLE IF NOT EXISTS system_settings (
--     setting_key VARCHAR(100) PRIMARY KEY,
--     setting_value TEXT NOT NULL,
--     description TEXT,
--     created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
--     updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
-- );

-- INSERT INTO system_settings (setting_key, setting_value, description) VALUES 
--     ('app_version', '1.0.0', 'Версия приложения'),
--     ('max_users', '1000', 'Максимальное количество пользователей'),
--     ('password_policy', 'simple', 'Политика паролей')
-- ON CONFLICT (setting_key) DO NOTHING;
