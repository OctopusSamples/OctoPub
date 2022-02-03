import {JWK} from "jwk-to-pem";

/**
 * Represents the configuration in the config.json file, which is processed by Octopus for each deployment
 * and environment.
 */
export interface DynamicConfig {
    settings: {
        productEndpoint: string;
        auditEndpoint: string;
        healthEndpoint: string;
        title: string;
        google: {
            tag: string;
            oauthClientId: string;
        },
        aws: {
            cognitoLogin: string;
            jwk: {
                keys: JWK[]
            };
        }
    },
    useDefaultTheme?: boolean;
    partition: string | null;
    setPartition: (id: string | null) => void;
    setAllBookId: (id: string | null) => void;
    allBookId: string | null;
}