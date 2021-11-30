import {FC, ReactElement, useContext, useEffect, useMemo, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {AppContext} from "../App";
import {getJson} from "../utils/network";
import { Clear, Done } from "@material-ui/icons";
import {createStyles, makeStyles} from "@material-ui/core/styles";
import {Theme} from "@material-ui/core";

const newHealth: {[key: string]: boolean} = {};

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        table: {
            height: "fit-content",
            marginRight: "auto",
            marginLeft: "auto"
        },
        cell: {
            padding: "8px"
        }
    })
);

const Book: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);

    const classes = useStyles();

    context.setAllBookId(null);

    const endpoints = useMemo(() => [
        context.settings.healthEndpoint + "/products/GET",
        context.settings.healthEndpoint + "/products/POST",
        context.settings.healthEndpoint + "/products/x/GET",
        context.settings.healthEndpoint + "/products/x/PATCH",
        context.settings.healthEndpoint + "/products/x/DELETE",
        context.settings.healthEndpoint + "/audits/GET",
        context.settings.healthEndpoint + "/audits/POST",
        context.settings.healthEndpoint + "/audits/x/GET"],
        [context]);

    const [health, setHealth] = useState<{[key: string]: boolean}>({});

    useEffect(() => {
        for (const endpoint of endpoints) {
            ((myEndpoint) => getJson(myEndpoint)
                .then(() => newHealth[myEndpoint] = true)
                .catch(() => newHealth[myEndpoint] = false)
                .finally(() => setHealth({...newHealth})))(endpoint);
        }
    }, [endpoints, setHealth]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title} - Health
                </title>
            </Helmet>
            <table className={classes.table}>
            {Object.entries(health)
                .sort()
                .map(([key, value]) =>
                <tr>
                    <td className={classes.cell}>{key.substring(key.lastIndexOf("/health"))}</td>
                    <td className={classes.cell}>{value ? <Done/> : <Clear/>}</td>
                </tr>
            )}
            </table>
        </>
    );
}

export default Book;