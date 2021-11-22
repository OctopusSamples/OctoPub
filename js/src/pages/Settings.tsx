import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {createStyles, FormLabel, Grid, makeStyles, TextField} from "@material-ui/core";
import {AppContext} from "../App";

const useStyles = makeStyles((theme) =>
    createStyles({
        container: {
            height: "fit-content",
            color: theme.palette.text.primary
        },
        cell: {
            padding: "8px"
        }
    })
);

const Settings: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);
    const classes = useStyles();
    const [apiKey, setApiKey] = useState<string | null>(localStorage.getItem("apiKey"));
    const [partition, setPartition] = useState<string | null>(localStorage.getItem("partition") || "main");

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true} className={classes.container}>
                <Grid className={classes.cell} md={2} sm={12} xs={12}>
                    <FormLabel>API Key</FormLabel>
                </Grid>
                <Grid className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id="apiKey" fullWidth={true} type="password" variant="outlined" value={apiKey}
                               onChange={v => {
                                   setApiKey(v.target.value);
                                   localStorage.setItem("apiKey", v.target.value);
                               }}/>
                </Grid>
                <Grid className={classes.cell} md={2} sm={12} xs={12}>
                    <FormLabel>Partition</FormLabel>
                </Grid>
                <Grid className={classes.cell} item md={10} sm={12} xs={12}>
                    <TextField id="partition" fullWidth={true} variant="outlined" value={partition}
                               onChange={v => {
                                   setPartition(v.target.value);
                                   localStorage.setItem("partition", v.target.value.trim());
                               }}/>
                </Grid>
            </Grid>

        </>
    );
}


export default Settings;