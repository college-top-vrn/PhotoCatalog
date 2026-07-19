CREATE TABLE photos
(
    id        UUID PRIMARY KEY DEFAULT uuidv7(),
    --Написать нужные для бизнеса метаданные,
    user_id   UUID  NOT NULL,
    photo_key TEXT  NOT NULL,
    metadata  JSONB NOT NULL   DEFAULT '{}'::jsonb,
    CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT unq_photo_key UNIQUE (photo_key)
);

CREATE INDEX idx_metadata ON photos USING gin (metadata)