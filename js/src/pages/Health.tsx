import {FC, ReactElement, useContext, useEffect, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {Theme} from "@material-ui/core";
import {AppContext} from "../App";
import {createStyles, makeStyles} from "@material-ui/core/styles";
import {getJson} from "../utils/network";

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        image: {
            objectFit: "contain",
            padding: "64px",
            width: "100%"
        },
        content: {

            "& a": {
                color: theme.palette.text.primary
            }
        }
    })
);

const Book: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const classes = useStyles();

    const context = useContext(AppContext);

    context.setAllBookId(null);

    const endpoints = [context.settings.productEndpoint + "/GET",
        context.settings.productEndpoint + "/POST",
        context.settings.productEndpoint + "/x/GET",
        context.settings.productEndpoint + "/x/PATCH",
        context.settings.productEndpoint + "/x/DELETE",
        context.settings.auditEndpoint + "/GET",
        context.settings.auditEndpoint + "/POST",
        context.settings.auditEndpoint + "/x/GET"];

    const [health, setHealth] = useState<{}>(endpoints.reduce((previousValue: any, currentValue) => {
        previousValue[currentValue] = false;
        return previousValue;
    }, {}));

    useEffect(() => {

        for (const endpoint in endpoints) {
            getJson(endpoint)
                .then(() => setHealth({}));
        }


    }, [endpoints, health, setHealth, endpoints]);

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title} - Health
                </title>
            </Helmet>


        </>
    );
}

export default Book;