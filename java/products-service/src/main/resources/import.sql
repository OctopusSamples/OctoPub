CREATE TABLE Product
(
    id     VARCHAR(255) NOT NULL,
    tenant VARCHAR(255) NULL,
    name   VARCHAR(255) NULL,
    CONSTRAINT pk_product PRIMARY KEY (id)
);

INSERT INTO Product (id, tenant, name)
VALUES (
        'urn:products:1',
        'main',
        'The 10 Pillars of Pragmatic Kubernetes Deployments');