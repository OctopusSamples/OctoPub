import {FC, ReactElement, useContext, useState} from "react";
import {CommonProps} from "../model/RouteItem.model";
import {Helmet} from "react-helmet";
import {AppContext} from "../App";
import {DataGrid, GridCellEditCommitParams, GridRowId} from "@material-ui/data-grid";
import {styles} from "../utils/styles";
import {Button, Grid} from "@material-ui/core";

interface RedirectRule {
    id: number,
    path: string,
    destination: string
}

const Branching: FC<CommonProps> = (props: CommonProps): ReactElement => {

    const context = useContext(AppContext);
    const classes = styles();

    context.setAllBookId(null);

    const [rules, setRules] = useState<RedirectRule[]>(JSON.parse(localStorage.getItem("branching") || "[]"));

    const columns = [
        {field: 'id', headerName: 'Index', width: 30},
        {field: 'path', headerName: 'Path', editable: true, width: 140},
        {field: 'destination', headerName: 'Subject', editable: true, width: 140}
    ];

    let selectedRows: GridRowId[] = [];

    return (
        <>
            <Helmet>
                <title>
                    {context.settings.title}
                </title>
            </Helmet>
            <Grid container={true} className={classes.container}>
                <Grid className={classes.cell} xs={12}>
                    <DataGrid
                        style={{height: "60vh"}}
                        rows={rules}
                        columns={columns}
                        pageSize={10}
                        rowsPerPageOptions={[10]}
                        onSelectionModelChange={(selection) => selectedRows = selection}
                        onCellEditCommit={onEdit}
                    />
                </Grid>
                <Grid container={true} className={classes.cell} sm={3} xs={12}>
                    <Button variant={"outlined"} onClick={_ => addRule()}>Add Rule</Button>
                </Grid>
                <Grid container={true} className={classes.cell} sm={3} xs={12}>
                    <Button variant={"outlined"} onClick={_ => deleteRule()}>Delete Rule</Button>
                </Grid>
            </Grid>
        </>
    );

    function onEdit(params: GridCellEditCommitParams) {
        for (let i = 0; i < rules.length; ++i) {
            if (rules[i].id === params.id) {
                if (params.field === "path") {
                    rules[i].path = params.value?.toString() || "";
                } else if (params.field === "destination") {
                    rules[i].destination = params.value?.toString() || "";
                }
            }
        }
        localStorage.setItem("branching", JSON.stringify(rules));
        setRules([...rules])
    }

    function addRule() {
        const newRules = [...rules, {id: rules.length, path: "", destination: ""}];
        localStorage.setItem("branching", JSON.stringify(newRules));
        setRules([...newRules])
    }

    function deleteRule() {
        const newRules = rules.filter(r => !selectedRows.some(s => s === r.id));
        localStorage.setItem("branching", JSON.stringify(newRules));
        setRules([...newRules])
    }
}



export default Branching;